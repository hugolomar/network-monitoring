# Traffic Composer Guide

`traffic_compose.py` executes replay scenarios defined in YAML/JSON files.

## What it does

1. Loads the scenario file.
2. Validates required fields (`name`, `duration_seconds`, `sources`, `tracks`).
3. Resolves each source path to a real PCAP file.
4. Expands repeating tracks (`repeat_every_seconds` + `repeat_count`) into timed events.
5. Runs event commands with `tcpreplay` according to event schedule.
6. Writes run artifacts to `tools/traffic/output/runs/<timestamp>/`.

## Commands

```bash
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --validate
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --dry-run
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --run
```

## Scenario model

- `sources`: named map of PCAP files.
- `tracks`: replay instructions over time.
  - `source`: source key from `sources`
  - `start_second`: initial offset from scenario start
  - `pps`: packets per second for this track
  - `iface` (optional): network interface override
  - one of:
    - `duration_seconds` (run until timeout)
    - `loop_count` (fixed number of loops)
  - optional repetition:
    - `repeat_every_seconds`
    - `repeat_count`

## Notes

- `duration_seconds` tracks rely on the host `timeout` command.
- Composer runs on host shell (not inside Docker).
- `validate` and `dry-run` do not send traffic.

## Build a deterministic PCAP from a scenario

Use this when the probe runs in `DeterministicTest` mode and you want one PCAP file
that follows the scenario timeline (`start_second`, overlaps, duration/loop tracks).

```bash
./tools/traffic/run.sh build-pcap --file tools/traffic/scenarios/five-minute-mac-coverage.yaml --dry-run
./tools/traffic/run.sh build-pcap --file tools/traffic/scenarios/five-minute-mac-coverage.yaml
./tools/traffic/run.sh build-all-pcaps
./tools/traffic/run.sh validate-pcaps
```

Default output path:

```text
tools/traffic/pcaps/generated/<scenario-name>.pcap
```

Generated PCAPs are kept separate from the external source catalog under `pcaps/external/`.

Validate scheduling before running the probe:

```bash
./tools/traffic/run.sh validate-pcaps
./tools/traffic/run.sh validate-pcaps --file tools/traffic/scenarios/five-minute-mac-coverage.yaml
```

Checks per scenario:

- total packet count matches tcpreplay scheduling (`loop_count` / `pps * duration_seconds`)
- first packet starts at t=0 after normalization
- last packet ends at the expected scenario timeline bound

Implementation: `tools/traffic/composer/traffic_scenario_pcap.py`

Behavior:

- Reuses the same scenario expansion logic as `traffic_compose.py`.
- Applies tcpreplay-equivalent scheduling per track:
  - `loop_count`: replays the full source PCAP that many times at `pps`.
  - `duration_seconds`: emits `pps * duration_seconds` packets, looping the source as needed.
- Merges all tracks with `mergecap` (overlaps interleave by timestamp).
