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
