# Traffic Lab

Traffic replay and scenario composition tooling for probe observability validation.

Execution model:

- Replay mode runs `tcpreplay` on host (WSL/Linux shell).
- Composer mode orchestrates timed multi-source replay tracks.
- A running probe/capture process is required.
- Backend/UI stack is recommended for end-to-end downstream validation.

Replay mode (ad-hoc regression):

```bash
./tools/traffic/run.sh replay --pcap tools/traffic/pcaps/external/dns.cap --pps 200 --loop 2
```

Composer mode (timeline-driven scenarios):

```bash
python3 -m pip install -r tools/traffic/composer/requirements.txt
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --validate
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --dry-run
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --run
```

Deterministic PCAP mode (single PCAP for probe `DeterministicTest` input):

```bash
./tools/traffic/run.sh build-pcap --file tools/traffic/scenarios/five-minute-mac-coverage.yaml --dry-run
./tools/traffic/run.sh build-pcap --file tools/traffic/scenarios/five-minute-mac-coverage.yaml
./tools/traffic/run.sh build-all-pcaps
./tools/traffic/run.sh validate-pcaps
```

Default output: `tools/traffic/pcaps/generated/<scenario-name>.pcap`

Validate generated PCAPs against tcpreplay-equivalent scheduling (packet counts and timeline bounds) before running the probe.

## Replay interface selection

Default interface resolution order:

1. `--iface` argument
2. `TRAFFIC_INTERFACE` environment variable
3. fallback `eth0`

## Documentation

- End-to-end deterministic probe flow: `docs/guides/traffic-deterministic-probe.md`
- Detailed operations: `tools/traffic/docs/traffic-lab.md`
- Replay-specific runbook: `tools/traffic/docs/pcap-replay.md`
- Composer flow guide: `tools/traffic/docs/composer-guide.md`
- Expected validation output: `tools/traffic/validation/expected-signals.md`
- Composer implementation: `tools/traffic/composer/traffic_compose.py`

## Notes

- External PCAP files under `tools/traffic/pcaps/external` are ignored by Git.
- Scenario-built PCAP files under `tools/traffic/pcaps/generated` are ignored by Git.
- Composer scenarios are the canonical path for repeatable long runs.
