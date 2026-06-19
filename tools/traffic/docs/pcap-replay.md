# PCAP Replay Runbook

This mode is intended for deterministic regression checks. For long realistic runs, use timeline scenarios with `run.sh scenario`.

## Direct replay mode

Run an explicit capture file:

```bash
./tools/traffic/run.sh replay --pcap tools/traffic/pcaps/external/http.cap --pps 200 --loop 1
```

## Parameters

- `--pcap`: required file path
- `--iface`: target interface (optional, default from resolver)
- `--pps`: replay throughput in packets-per-second
- `--loop`: replay iteration count
- `--dry-run`: print command only

## Important precondition

Replay alone does not validate ingestion. Start the probe/capturadora first, and ensure it captures the same interface used in replay (`--iface` or `TRAFFIC_INTERFACE`).

## Troubleshooting

- `Required command not found: tcpreplay`
  - Install package in your environment (`sudo apt-get install tcpreplay` in Debian/Ubuntu)
- `Required file not found`
  - Verify file exists under `tools/traffic/pcaps/external`
- No visible pipeline changes
  - Lower `--pps` and increase `--loop`
  - Verify probe is running and listening on the same interface
  - Run `tools/traffic/validation/smoke-checks.sh`
