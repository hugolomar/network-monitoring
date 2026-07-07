#!/usr/bin/env python3
"""Declarative traffic composer for repeatable replay scenarios.

Execution lifecycle:
    1. Load scenario (`.yaml` / `.json`).
    2. Validate fields and resolve PCAP paths.
    3. Expand repeating tracks into concrete timed events.
    4. Print execution plan (`validate` / `dry-run`) or execute with
       `tcpreplay` (`run`).
    5. Persist run summary under `tools/traffic/output/runs/<timestamp>/`.
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import signal
import subprocess
import sys
import time
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


@dataclass(frozen=True)
class Event:
    """Single replay event resolved from scenario tracks.

    Attributes:
        name: Concrete event name (for example `burst#2`).
        source: Logical source key from scenario `sources`.
        pcap_path: Absolute path to the PCAP file used by the event.
        start_second: Offset from scenario start when event should begin.
        pps: Replay throughput in packets per second.
        iface: Network interface used by `tcpreplay`.
        duration_seconds: Duration-based execution window, if configured.
        loop_count: Loop-based execution count, if configured.
    """

    name: str
    source: str
    pcap_path: Path
    start_second: int
    pps: int
    iface: str
    duration_seconds: int | None
    loop_count: int | None


def _load_yaml_if_available(path: Path) -> dict[str, Any]:
    """Load a YAML scenario file when PyYAML is available.

    Args:
        path: Absolute or relative path to the scenario file.

    Returns:
        Parsed scenario as a dictionary.

    Raises:
        SystemExit: If PyYAML is not installed or YAML root is invalid.
    """
    try:
        import yaml  # type: ignore
    except ModuleNotFoundError as exc:
        raise SystemExit(
            "PyYAML is required to load YAML scenarios. "
            "Install it with: python3 -m pip install -r tools/traffic/composer/requirements.txt"
        ) from exc

    with path.open("r", encoding="utf-8") as handle:
        data = yaml.safe_load(handle)
    if not isinstance(data, dict):
        raise SystemExit(f"Scenario root must be a mapping: {path}")
    return data


def load_scenario(path: Path) -> dict[str, Any]:
    """Load a scenario file from JSON or YAML.

    Args:
        path: Path to a `.json`, `.yaml`, or `.yml` scenario file.

    Returns:
        Parsed scenario dictionary.

    Raises:
        SystemExit: If the file does not exist, format is unsupported,
            or root object is invalid.
    """
    if not path.exists():
        raise SystemExit(f"Scenario file not found: {path}")

    suffix = path.suffix.lower()
    if suffix == ".json":
        with path.open("r", encoding="utf-8") as handle:
            data = json.load(handle)
        if not isinstance(data, dict):
            raise SystemExit(f"Scenario root must be an object: {path}")
        return data

    if suffix in {".yaml", ".yml"}:
        return _load_yaml_if_available(path)

    raise SystemExit(f"Unsupported scenario extension '{suffix}'. Use .json/.yaml/.yml")


def _require_int(value: Any, field_name: str, min_value: int = 0) -> int:
    """Validate an integer config field.

    Args:
        value: Value to validate.
        field_name: Human-readable field path for error messages.
        min_value: Inclusive minimum accepted value.

    Returns:
        The validated integer value.

    Raises:
        SystemExit: If the value is not an integer or below `min_value`.
    """
    if not isinstance(value, int):
        raise SystemExit(f"Field '{field_name}' must be an integer")
    if value < min_value:
        raise SystemExit(f"Field '{field_name}' must be >= {min_value}")
    return value


def _resolve_source_pcap(traffic_root: Path, source_path: str) -> Path:
    """Resolve a source PCAP path against traffic root.

    Args:
        traffic_root: `tools/traffic` absolute directory.
        source_path: Scenario source path (relative or absolute).

    Returns:
        Absolute path to an existing PCAP file.

    Raises:
        SystemExit: If the resolved file does not exist.
    """
    path = Path(source_path)
    if not path.is_absolute():
        path = (traffic_root / source_path).resolve()
    if not path.exists():
        raise SystemExit(f"Source PCAP not found: {path}")
    return path


def _expand_track_events(
    *,
    track: dict[str, Any],
    track_name: str,
    source_name: str,
    source_pcap: Path,
    default_iface: str,
    scenario_duration: int,
) -> list[Event]:
    """Expand a track definition into one or many concrete events.

    Args:
        track: Raw track object from scenario configuration.
        track_name: Track name used for diagnostics and event naming.
        source_name: Source key referenced by the track.
        source_pcap: Resolved PCAP path for `source_name`.
        default_iface: Scenario-level default interface.
        scenario_duration: Total scenario duration in seconds.

    Returns:
        List of resolved events generated from the track.

    Raises:
        SystemExit: If track configuration is inconsistent or out of bounds.
    """
    start_second = _require_int(track.get("start_second", 0), f"tracks.{track_name}.start_second", 0)
    pps = _require_int(track.get("pps"), f"tracks.{track_name}.pps", 1)
    iface = track.get("iface", default_iface)
    if not isinstance(iface, str) or not iface:
        raise SystemExit(f"Field 'tracks.{track_name}.iface' must be a non-empty string")

    duration_value = track.get("duration_seconds")
    if duration_value is not None:
        duration_seconds = _require_int(duration_value, f"tracks.{track_name}.duration_seconds", 1)
    else:
        duration_seconds = None

    loop_value = track.get("loop_count")
    if loop_value is not None:
        loop_count = _require_int(loop_value, f"tracks.{track_name}.loop_count", 1)
    else:
        loop_count = None

    if duration_seconds is None and loop_count is None:
        raise SystemExit(
            f"Track '{track_name}' must define either duration_seconds or loop_count"
        )

    if duration_seconds is not None and loop_count is not None:
        raise SystemExit(
            f"Track '{track_name}' cannot define both duration_seconds and loop_count"
        )

    repeat_every = track.get("repeat_every_seconds")
    repeat_count = track.get("repeat_count")
    starts: list[int] = [start_second]
    if repeat_every is not None:
        repeat_every_seconds = _require_int(
            repeat_every, f"tracks.{track_name}.repeat_every_seconds", 1
        )
        if repeat_count is None:
            raise SystemExit(
                f"Track '{track_name}' defines repeat_every_seconds but missing repeat_count"
            )
        repeats = _require_int(repeat_count, f"tracks.{track_name}.repeat_count", 1)
        starts = [start_second + (idx * repeat_every_seconds) for idx in range(repeats)]
    elif repeat_count is not None:
        raise SystemExit(
            f"Track '{track_name}' defines repeat_count without repeat_every_seconds"
        )

    events: list[Event] = []
    for idx, event_start in enumerate(starts, start=1):
        if event_start >= scenario_duration:
            raise SystemExit(
                f"Track '{track_name}' event {idx} starts beyond scenario duration ({scenario_duration}s)"
            )
        if duration_seconds is not None and event_start + duration_seconds > scenario_duration:
            raise SystemExit(
                f"Track '{track_name}' event {idx} exceeds scenario duration ({scenario_duration}s)"
            )
        event_name = track_name if len(starts) == 1 else f"{track_name}#{idx}"
        events.append(
            Event(
                name=event_name,
                source=source_name,
                pcap_path=source_pcap,
                start_second=event_start,
                pps=pps,
                iface=iface,
                duration_seconds=duration_seconds,
                loop_count=loop_count,
            )
        )
    return events


def build_events(config: dict[str, Any], traffic_root: Path) -> tuple[str, int, str, list[Event]]:
    """Validate scenario config and produce a sorted event list.

    Args:
        config: Parsed scenario object.
        traffic_root: `tools/traffic` absolute directory.

    Returns:
        Tuple with scenario name, total duration, default iface,
        and chronologically sorted events.

    Raises:
        SystemExit: If scenario structure or values are invalid.
    """
    name = config.get("name")
    if not isinstance(name, str) or not name:
        raise SystemExit("Field 'name' must be a non-empty string")

    duration_seconds = _require_int(config.get("duration_seconds"), "duration_seconds", 1)
    default_iface = config.get("default_iface", os.environ.get("TRAFFIC_INTERFACE", "eth0"))
    if not isinstance(default_iface, str) or not default_iface:
        raise SystemExit("Field 'default_iface' must be a non-empty string")

    sources = config.get("sources")
    if not isinstance(sources, dict) or not sources:
        raise SystemExit("Field 'sources' must be a non-empty mapping")

    resolved_sources: dict[str, Path] = {}
    for source_name, source_path in sources.items():
        if not isinstance(source_name, str) or not source_name:
            raise SystemExit("Source names must be non-empty strings")
        if not isinstance(source_path, str) or not source_path:
            raise SystemExit(f"Source '{source_name}' path must be a non-empty string")
        resolved_sources[source_name] = _resolve_source_pcap(traffic_root, source_path)

    tracks = config.get("tracks")
    if not isinstance(tracks, list) or not tracks:
        raise SystemExit("Field 'tracks' must be a non-empty list")

    all_events: list[Event] = []
    for idx, track in enumerate(tracks):
        if not isinstance(track, dict):
            raise SystemExit(f"Track at index {idx} must be an object")
        track_name = track.get("name", f"track-{idx+1}")
        if not isinstance(track_name, str) or not track_name:
            raise SystemExit(f"Track at index {idx} has invalid 'name'")
        source_name = track.get("source")
        if not isinstance(source_name, str) or source_name not in resolved_sources:
            raise SystemExit(
                f"Track '{track_name}' references unknown source '{source_name}'"
            )
        events = _expand_track_events(
            track=track,
            track_name=track_name,
            source_name=source_name,
            source_pcap=resolved_sources[source_name],
            default_iface=default_iface,
            scenario_duration=duration_seconds,
        )
        all_events.extend(events)

    all_events.sort(key=lambda event: (event.start_second, event.name))
    return name, duration_seconds, default_iface, all_events


def _event_command(event: Event) -> list[str]:
    """Build the command line used to execute a replay event.

    Args:
        event: Resolved event to execute.

    Returns:
        Full command token list for `subprocess.Popen`.
    """
    base = [
        "tcpreplay",
        "--intf1",
        event.iface,
        "--pps",
        str(event.pps),
    ]
    if event.duration_seconds is not None:
        return [
            "timeout",
            "--signal=INT",
            str(event.duration_seconds),
            *base,
            "--loop",
            "0",
            str(event.pcap_path),
        ]
    return [*base, "--loop", str(event.loop_count), str(event.pcap_path)]


def print_plan(name: str, duration_seconds: int, events: list[Event]) -> None:
    """Render a human-readable execution plan.

    Args:
        name: Scenario name.
        duration_seconds: Scenario duration in seconds.
        events: Resolved event list.
    """
    print(f"Scenario: {name}")
    print(f"Duration: {duration_seconds}s")
    print("Plan:")
    for event in events:
        mode = (
            f"duration={event.duration_seconds}s"
            if event.duration_seconds is not None
            else f"loop={event.loop_count}"
        )
        print(
            f"  - t+{event.start_second:>4}s | {event.name:<22} "
            f"| src={event.source:<12} | pps={event.pps:<5} | {mode}"
        )
        cmd_preview = " ".join(_event_command(event))
        print(f"      {cmd_preview}")


def _write_run_summary(
    *,
    traffic_root: Path,
    scenario_name: str,
    scenario_duration: int,
    events: list[Event],
    results: list[dict[str, Any]],
) -> Path:
    """Persist run artifacts for later inspection.

    Args:
        traffic_root: `tools/traffic` absolute directory.
        scenario_name: Executed scenario name.
        scenario_duration: Scenario duration in seconds.
        events: Executed event definitions.
        results: Per-event runtime outcomes.

    Returns:
        Absolute path to the run artifact directory.
    """
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    run_dir = traffic_root / "output" / "runs" / timestamp
    run_dir.mkdir(parents=True, exist_ok=True)

    payload = {
        "scenario_name": scenario_name,
        "scenario_duration_seconds": scenario_duration,
        "started_at_utc": timestamp,
        "events": [
            {
                "name": event.name,
                "source": event.source,
                "pcap": str(event.pcap_path),
                "start_second": event.start_second,
                "pps": event.pps,
                "iface": event.iface,
                "duration_seconds": event.duration_seconds,
                "loop_count": event.loop_count,
            }
            for event in events
        ],
        "results": results,
    }
    summary_json = run_dir / "summary.json"
    with summary_json.open("w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2)

    summary_md = run_dir / "summary.md"
    with summary_md.open("w", encoding="utf-8") as handle:
        handle.write(f"# Traffic Composer Run\n\n")
        handle.write(f"- Scenario: `{scenario_name}`\n")
        handle.write(f"- Duration: `{scenario_duration}` seconds\n")
        handle.write(f"- Result entries: `{len(results)}`\n\n")
        handle.write("## Event results\n\n")
        for result in results:
            handle.write(
                f"- `{result['name']}`: exit={result['exit_code']} "
                f"started_at={result['started_at_second']}s "
                f"finished_at={result['finished_at_second']}s\n"
            )

    return run_dir


def run_events(
    *,
    scenario_name: str,
    scenario_duration: int,
    events: list[Event],
    traffic_root: Path,
) -> int:
    """Execute scheduled replay events and collect results.

    Args:
        scenario_name: Scenario name for logs and artifacts.
        scenario_duration: Scenario duration in seconds.
        events: Chronologically sorted events to execute.
        traffic_root: `tools/traffic` absolute directory.

    Returns:
        Process-compatible exit code:
            - `0` when all events complete successfully.
            - `1` when at least one event exits with non-zero status.
            - `2` when required host tools are missing.
            - `130` when execution is interrupted by user.
    """
    if shutil.which("tcpreplay") is None:
        print("ERROR: 'tcpreplay' is required in PATH", file=sys.stderr)
        return 2

    if any(event.duration_seconds is not None for event in events) and shutil.which("timeout") is None:
        print("ERROR: 'timeout' command is required for duration-based tracks", file=sys.stderr)
        return 2

    print_plan(scenario_name, scenario_duration, events)
    print("\nStarting scenario execution...\n")

    start_monotonic = time.monotonic()
    pending = list(events)
    running: list[tuple[Event, subprocess.Popen[str], float]] = []
    results: list[dict[str, Any]] = []

    try:
        while pending or running:
            elapsed = time.monotonic() - start_monotonic
            # Start every event whose schedule threshold is reached.
            while pending and pending[0].start_second <= elapsed:
                event = pending.pop(0)
                command = _event_command(event)
                process = subprocess.Popen(command, text=True)
                running.append((event, process, elapsed))
                print(
                    f"[t+{int(elapsed):>4}s] started {event.name} "
                    f"(pid={process.pid}, pps={event.pps}, iface={event.iface})"
                )

            # Poll active events and record completion metadata.
            for event, process, started_at in list(running):
                exit_code = process.poll()
                if exit_code is None:
                    continue
                running.remove((event, process, started_at))
                ended_at = time.monotonic() - start_monotonic
                results.append(
                    {
                        "name": event.name,
                        "source": event.source,
                        "exit_code": exit_code,
                        "started_at_second": int(started_at),
                        "finished_at_second": int(ended_at),
                    }
                )
                print(
                    f"[t+{int(ended_at):>4}s] finished {event.name} "
                    f"(exit={exit_code})"
                )

            time.sleep(0.2)
    except KeyboardInterrupt:
        print("\nInterrupted. Terminating running replay processes...", file=sys.stderr)
        for _, process, _ in running:
            process.send_signal(signal.SIGINT)
        for _, process, _ in running:
            process.wait(timeout=5)
        return 130

    run_dir = _write_run_summary(
        traffic_root=traffic_root,
        scenario_name=scenario_name,
        scenario_duration=scenario_duration,
        events=events,
        results=results,
    )
    print(f"\nScenario finished. Run artifacts: {run_dir}")

    failed = [result for result in results if result["exit_code"] != 0]
    return 1 if failed else 0


def build_parser() -> argparse.ArgumentParser:
    """Create CLI parser for composer commands.

    Returns:
        Configured `argparse.ArgumentParser` instance.
    """
    parser = argparse.ArgumentParser(description="Compose traffic replay scenarios")
    subparsers = parser.add_subparsers(dest="command", required=True)

    for cmd_name in ("validate", "dry-run", "run"):
        cmd = subparsers.add_parser(cmd_name)
        cmd.add_argument(
            "--file",
            required=True,
            help="Scenario file path (.yaml/.yml/.json)",
        )
    return parser


def main() -> int:
    """Execute CLI command flow.

    Returns:
        Process-compatible exit code.
    """
    parser = build_parser()
    args = parser.parse_args()

    script_path = Path(__file__).resolve()
    traffic_root = script_path.parent.parent
    scenario_path = Path(args.file).resolve()
    config = load_scenario(scenario_path)
    scenario_name, scenario_duration, _, events = build_events(config, traffic_root)

    if args.command in {"validate", "dry-run"}:
        print_plan(scenario_name, scenario_duration, events)
        print("\nValidation successful.")
        return 0

    return run_events(
        scenario_name=scenario_name,
        scenario_duration=scenario_duration,
        events=events,
        traffic_root=traffic_root,
    )


if __name__ == "__main__":
    raise SystemExit(main())
