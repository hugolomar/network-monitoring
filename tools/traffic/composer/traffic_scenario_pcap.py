#!/usr/bin/env python3
"""Build a single PCAP from a traffic scenario YAML timeline.

The output PCAP is intended for probe `DeterministicTest` mode: each track is placed
at its configured `start_second` and throttled to its configured `pps`, matching the
tcpreplay scheduling model used by `traffic_compose.py`. Overlapping tracks are merged
by packet time via `mergecap`.
"""

from __future__ import annotations

import argparse
import shutil
import struct
import subprocess
import sys
import tempfile
from dataclasses import dataclass
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(SCRIPT_DIR))

from traffic_compose import Event, build_events, load_scenario, print_plan  # noqa: E402

PCAPNG_MAGIC = 0x0A0D0D0A
PCAP_MAGIC_LE = 0xA1B2C3D4
PCAP_MAGIC_BE = 0xD4C3B2A1
PCAP_NSEC_MAGIC_LE = 0xA1B23C4D
PCAP_NSEC_MAGIC_BE = 0x4D3CB2A1


@dataclass(frozen=True)
class RawPacket:
    data: bytes


def _require_tool(name: str) -> None:
    if shutil.which(name) is None:
        raise SystemExit(f"ERROR: required command not found in PATH: {name}")


def _run(command: list[str]) -> None:
    completed = subprocess.run(command, capture_output=True, text=True, check=False)
    if completed.returncode != 0:
        stderr = completed.stderr.strip()
        stdout = completed.stdout.strip()
        detail = stderr or stdout or f"exit={completed.returncode}"
        raise SystemExit(f"Command failed: {' '.join(command)}\n{detail}")


def _first_packet_epoch(pcap_path: Path) -> float:
    command = [
        "tshark",
        "-r",
        str(pcap_path),
        "-c",
        "1",
        "-T",
        "fields",
        "-e",
        "frame.time_epoch",
    ]
    completed = subprocess.run(command, capture_output=True, text=True, check=False)
    if completed.returncode != 0:
        raise SystemExit(f"Unable to read first packet timestamp: {pcap_path}")

    value = completed.stdout.strip()
    if not value:
        raise SystemExit(f"PCAP has no readable packets: {pcap_path}")
    try:
        return float(value)
    except ValueError as exc:
        raise SystemExit(f"Invalid first packet timestamp in {pcap_path}: {value}") from exc


def _merge_pcaps(output_path: Path, inputs: list[Path]) -> None:
    if len(inputs) == 1:
        shutil.copyfile(inputs[0], output_path)
        return
    _run(["mergecap", "-w", str(output_path), *[str(path) for path in inputs]])


def _shift_timestamps(source: Path, output_path: Path, offset_seconds: float) -> None:
    if abs(offset_seconds) < 1e-9:
        shutil.copyfile(source, output_path)
        return
    _run(["editcap", "-t", f"{offset_seconds}", str(source), str(output_path)])


def _normalize_timestamps_to_zero(source: Path, output_path: Path) -> None:
    first_epoch = _first_packet_epoch(source)
    _shift_timestamps(source, output_path, -first_epoch)


def _ensure_classic_pcap(source: Path, output_path: Path) -> Path:
    with source.open("rb") as handle:
        magic = int.from_bytes(handle.read(4), "little")
    if magic == PCAPNG_MAGIC:
        _run(["editcap", "-F", "pcap", str(source), str(output_path)])
        return output_path
    return source


def _read_pcap_packets(path: Path) -> tuple[int, list[RawPacket]]:
    with path.open("rb") as handle:
        header = handle.read(24)
        if len(header) < 24:
            raise SystemExit(f"Invalid PCAP header: {path}")

        magic_le = struct.unpack("<I", header[:4])[0]
        if magic_le == PCAP_MAGIC_LE:
            endian = "<"
        elif magic_le == PCAP_MAGIC_BE:
            endian = ">"
        elif magic_le == PCAP_NSEC_MAGIC_LE:
            endian = "<"
        elif magic_le == PCAP_NSEC_MAGIC_BE:
            endian = ">"
        else:
            raise SystemExit(f"Unsupported PCAP format: {path}")

        _magic, _major, _minor, _zone, _sigfigs, _snaplen, network = struct.unpack(
            f"{endian}IHHIIII",
            header,
        )

        packet_header_fmt = f"{endian}IIII"
        packets: list[RawPacket] = []
        while True:
            packet_header = handle.read(16)
            if len(packet_header) < 16:
                break
            _ts_sec, _ts_frac, incl_len, _orig_len = struct.unpack(
                packet_header_fmt,
                packet_header,
            )
            data = handle.read(incl_len)
            if len(data) < incl_len:
                break
            packets.append(RawPacket(data=data))

        return network, packets


def _write_pcap_packets(
    path: Path,
    network: int,
    packets: list[tuple[float, RawPacket]],
) -> None:
    with path.open("wb") as handle:
        handle.write(struct.pack("<IHHIIII", PCAP_MAGIC_LE, 2, 4, 0, 0, 65_535, network))
        for timestamp, packet in packets:
            ts_sec = int(timestamp)
            ts_usec = int(round((timestamp - ts_sec) * 1_000_000))
            if ts_usec >= 1_000_000:
                ts_sec += ts_usec // 1_000_000
                ts_usec %= 1_000_000
            incl_len = len(packet.data)
            handle.write(struct.pack("<IIII", ts_sec, ts_usec, incl_len, incl_len))
            handle.write(packet.data)


def _load_source_packets(
    source: Path,
    work_dir: Path,
    cache: dict[Path, tuple[int, list[RawPacket]]],
) -> tuple[int, list[RawPacket]]:
    if source in cache:
        return cache[source]

    classic_path = work_dir / f"classic-{source.name}"
    readable = _ensure_classic_pcap(source, classic_path)
    network, packets = _read_pcap_packets(readable)
    if not packets:
        raise SystemExit(f"Source PCAP has no packets: {source}")

    cache[source] = (network, packets)
    return network, packets


def _packet_count_for_event(event: Event, source_packet_count: int) -> int:
    if event.loop_count is not None:
        return event.loop_count * source_packet_count

    assert event.duration_seconds is not None
    return event.pps * event.duration_seconds


def _build_event_segment(
    event: Event,
    work_dir: Path,
    index: int,
    source_cache: dict[Path, tuple[int, list[RawPacket]]],
) -> Path:
    final_path = work_dir / f"{index:03d}-{event.name}-segment.pcap"
    network, source_packets = _load_source_packets(event.pcap_path, work_dir, source_cache)

    packet_count = _packet_count_for_event(event, len(source_packets))
    interval_seconds = 1.0 / event.pps
    scheduled: list[tuple[float, RawPacket]] = []
    for packet_index in range(packet_count):
        timestamp = event.start_second + packet_index * interval_seconds
        source_packet = source_packets[packet_index % len(source_packets)]
        scheduled.append((timestamp, source_packet))

    _write_pcap_packets(final_path, network, scheduled)
    return final_path


def build_scenario_pcap(
    *,
    scenario_file: Path,
    output_path: Path,
    traffic_root: Path,
) -> tuple[str, int, list[Event]]:
    _require_tool("mergecap")
    _require_tool("editcap")
    _require_tool("tshark")

    config = load_scenario(scenario_file.resolve())
    scenario_name, scenario_duration, _, events = build_events(config, traffic_root)
    if not events:
        raise SystemExit("Scenario produced no events.")

    output_path.parent.mkdir(parents=True, exist_ok=True)

    with tempfile.TemporaryDirectory(prefix="traffic-scenario-pcap-") as temp_dir:
        work_dir = Path(temp_dir)
        source_cache: dict[Path, tuple[int, list[RawPacket]]] = {}
        segments = [
            _build_event_segment(event, work_dir, index, source_cache)
            for index, event in enumerate(events, start=1)
        ]
        merged_path = work_dir / "merged.pcap"
        _merge_pcaps(merged_path, segments)
        _normalize_timestamps_to_zero(merged_path, output_path)

    return scenario_name, scenario_duration, events


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Build a single PCAP that follows a scenario YAML timeline"
    )
    parser.add_argument(
        "--file",
        required=True,
        help="Scenario file path (.yaml/.yml/.json)",
    )
    parser.add_argument(
        "--output",
        help="Output PCAP path (default: pcaps/generated/<scenario-name>.pcap)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Validate scenario and print plan without writing PCAP",
    )
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    traffic_root = SCRIPT_DIR.parent
    scenario_path = Path(args.file).resolve()
    config = load_scenario(scenario_path)
    scenario_name, scenario_duration, _, events = build_events(config, traffic_root)

    if args.dry_run:
        print_plan(scenario_name, scenario_duration, events)
        print("\nDry run successful. No PCAP written.")
        return 0

    output_path = (
        Path(args.output).resolve()
        if args.output
        else traffic_root / "pcaps" / "generated" / f"{scenario_name}.pcap"
    )

    print_plan(scenario_name, scenario_duration, events)
    print(f"\nBuilding PCAP: {output_path}")

    build_scenario_pcap(
        scenario_file=scenario_path,
        output_path=output_path,
        traffic_root=traffic_root,
    )

    print(f"Scenario PCAP written: {output_path}")
    print("Tracks follow tcpreplay scheduling: start_second offsets and pps throttling.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
