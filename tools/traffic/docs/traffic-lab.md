# Traffic Lab Runbook

## Purpose

Provide a reproducible way to generate traffic signals for the probe/inventory/graph pipeline using:

- replayed PCAP datasets (ad-hoc mode), and
- declarative timeline scenarios (composer mode).

## Preconditions

- Probe/capturadora process running and listening on the replay interface
- PCAP files available in `tools/traffic/pcaps/external/`
- Optional but recommended for end-to-end validation: backend + UI + dependencies running
- `tcpreplay` installed on execution host (replay mode only)

## Execution model

- Replay scripts execute in host shell (WSL/Linux), not as Docker services.
- Replayed packets must be injected on an interface observed by the probe.
- If probe is not running, replay may complete successfully but no inventory/graph updates will appear.

## Replay mode commands

```bash
./tools/traffic/run.sh replay --pcap tools/traffic/pcaps/external/http.cap --pps 200 --loop 1
```

## Composer mode commands

Use a declarative timeline for long executions and mixed overlays:

```bash
python3 -m pip install -r tools/traffic/composer/requirements.txt
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --validate
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --dry-run
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --run
```

Composer run artifacts are written to `tools/traffic/output/runs/<timestamp>/`.
Composer internals are documented in `tools/traffic/docs/composer-guide.md`.

Use a different replay interface if needed:

```bash
./tools/traffic/run.sh replay --pcap tools/traffic/pcaps/external/http.cap --iface eth0
```

Dry-run command rendering:

```bash
./tools/traffic/run.sh replay --pcap tools/traffic/pcaps/external/http.cap --dry-run
```

## Validation flow

After each run:

1. Execute `tools/traffic/validation/smoke-checks.sh`
   - Graph check sends auth headers by default (`Authorization: Bearer test`, `X-Role: analyst`).
   - Override if needed with `GRAPH_AUTH_HEADER` and `GRAPH_ROLE_HEADER`.
2. Compare observations with `tools/traffic/validation/expected-signals.md`
3. If needed, inspect graph in UI (`http://localhost:3000`) and Neo4j browser (`http://localhost:7474`)
