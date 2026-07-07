#!/usr/bin/env python3
"""Validate generated scenario PCAPs against tcpreplay-equivalent scheduling."""

from __future__ import annotations

import argparse
import subprocess
import sys
import tempfile
from dataclasses import dataclass
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(SCRIPT_DIR))

from traffic_compose import Event, build_events, load_scenario  # noqa: E402
from traffic_scenario_pcap import (  # noqa: E402
    _load_source_packets,
    _packet_count_for_event,
)


@dataclass(frozen=True)
class ExpectedScenarioMetrics:
    total_packets: int
    max_relative_seconds: float


@dataclass(frozen=True)
class ValidationResult:
    scenario_file: Path
    pcap_path: Path
    ok: bool
    messages: tuple[str, ...]


def expected_metrics(
    events: list[Event],
    source_counts: dict[Path, int],
) -> ExpectedScenarioMetrics:
    total_packets = 0
    max_relative_seconds = 0.0
    for event in events:
        source_count = source_counts[event.pcap_path]
        packet_count = _packet_count_for_event(event, source_count)
        total_packets += packet_count
        if packet_count > 0:
            event_end = event.start_second + (packet_count - 1) / event.pps
            max_relative_seconds = max(max_relative_seconds, event_end)
    return ExpectedScenarioMetrics(
        total_packets=total_packets,
        max_relative_seconds=max_relative_seconds,
    )


def compute_expected(
    scenario_file: Path,
    traffic_root: Path,
    work_dir: Path,
) -> tuple[str, list[Event], ExpectedScenarioMetrics]:
    config = load_scenario(scenario_file.resolve())
    scenario_name, _, _, events = build_events(config, traffic_root)
    cache: dict = {}
    source_counts: dict[Path, int] = {}
    for event in events:
        if event.pcap_path not in source_counts:
            _, packets = _load_source_packets(event.pcap_path, work_dir, cache)
            source_counts[event.pcap_path] = len(packets)
    metrics = expected_metrics(events, source_counts)
    return scenario_name, events, metrics


def _read_relative_times(pcap_path: Path) -> list[float]:
    command = [
        "tshark",
        "-r",
        str(pcap_path),
        "-T",
        "fields",
        "-e",
        "frame.time_relative",
    ]
    completed = subprocess.run(command, capture_output=True, text=True, check=False)
    if completed.returncode != 0:
        detail = completed.stderr.strip() or completed.stdout.strip() or "tshark failed"
        raise RuntimeError(f"Unable to read PCAP timestamps: {pcap_path}\n{detail}")

    values: list[float] = []
    for line in completed.stdout.splitlines():
        value = line.strip()
        if not value:
            continue
        values.append(float(value))
    return values


def validate_generated_pcap(
    *,
    scenario_file: Path,
    pcap_path: Path,
    traffic_root: Path,
    work_dir: Path,
    time_tolerance_seconds: float = 0.01,
) -> ValidationResult:
    messages: list[str] = []
    ok = True

    try:
        scenario_name, _events, expected = compute_expected(scenario_file, traffic_root, work_dir)
    except SystemExit as exc:
        return ValidationResult(
            scenario_file=scenario_file,
            pcap_path=pcap_path,
            ok=False,
            messages=(str(exc),),
        )

    if not pcap_path.is_file():
        return ValidationResult(
            scenario_file=scenario_file,
            pcap_path=pcap_path,
            ok=False,
            messages=(f"Missing generated PCAP for scenario '{scenario_name}'.",),
        )

    try:
        relative_times = _read_relative_times(pcap_path)
    except RuntimeError as exc:
        return ValidationResult(
            scenario_file=scenario_file,
            pcap_path=pcap_path,
            ok=False,
            messages=(str(exc),),
        )

    actual_packets = len(relative_times)
    if actual_packets != expected.total_packets:
        ok = False
        messages.append(
            f"Packet count mismatch: expected={expected.total_packets} actual={actual_packets}"
        )
    else:
        messages.append(f"Packet count OK ({actual_packets})")

    if not relative_times:
        return ValidationResult(
            scenario_file=scenario_file,
            pcap_path=pcap_path,
            ok=False,
            messages=tuple(messages + ["PCAP contains no packets."]),
        )

    min_t = min(relative_times)
    max_t = max(relative_times)
    if min_t < -time_tolerance_seconds:
        ok = False
        messages.append(f"First packet too early: {min_t:.6f}s")
    else:
        messages.append(f"Start time OK ({min_t:.6f}s)")

    if abs(max_t - expected.max_relative_seconds) > time_tolerance_seconds:
        ok = False
        messages.append(
            "End time mismatch: "
            f"expected={expected.max_relative_seconds:.6f}s actual={max_t:.6f}s"
        )
    else:
        messages.append(f"End time OK ({max_t:.6f}s)")

    return ValidationResult(
        scenario_file=scenario_file,
        pcap_path=pcap_path,
        ok=ok,
        messages=tuple(messages),
    )


def default_generated_path(traffic_root: Path, scenario_name: str) -> Path:
    return traffic_root / "pcaps" / "generated" / f"{scenario_name}.pcap"


def discover_scenarios(traffic_root: Path) -> list[Path]:
    return sorted((traffic_root / "scenarios").glob("*.yaml"))


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Validate generated scenario PCAP scheduling metrics"
    )
    parser.add_argument(
        "--file",
        help="Scenario file path (.yaml). Default: validate all scenarios.",
    )
    parser.add_argument(
        "--pcap",
        help="Generated PCAP path (default: pcaps/generated/<scenario-name>.pcap)",
    )
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    traffic_root = SCRIPT_DIR.parent

    if args.file:
        scenario_files = [Path(args.file).resolve()]
    else:
        scenario_files = discover_scenarios(traffic_root)

    if not scenario_files:
        print("No scenario files found.", file=sys.stderr)
        return 1

    results: list[ValidationResult] = []
    with tempfile.TemporaryDirectory(prefix="traffic-validate-") as temp_dir:
        work_dir = Path(temp_dir)
        for scenario_file in scenario_files:
            config = load_scenario(scenario_file)
            scenario_name, _, _, _ = build_events(config, traffic_root)
            pcap_path = (
                Path(args.pcap).resolve()
                if args.pcap and args.file
                else default_generated_path(traffic_root, scenario_name)
            )

            result = validate_generated_pcap(
                scenario_file=scenario_file,
                pcap_path=pcap_path,
                traffic_root=traffic_root,
                work_dir=work_dir,
            )
            results.append(result)

    failed = 0
    for result in results:
        status = "OK" if result.ok else "FAIL"
        print(f"[{status}] {result.scenario_file.name} -> {result.pcap_path}")
        for message in result.messages:
            print(f"  - {message}")
        if not result.ok:
            failed += 1

    if failed:
        print(f"\nValidation failed for {failed} scenario(s).", file=sys.stderr)
        return 1

    print(f"\nValidation passed for {len(results)} scenario(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
