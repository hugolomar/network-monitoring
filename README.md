# Network Monitoring

Event-driven network monitoring platform: a passive probe captures traffic (`tshark`), derives
`SessionDetected`/`DeviceDetected` events, publishes them to Kafka (Avro + Schema Registry), and feeds
backend inventory and communication graph projections.

This README is the entrypoint for running the system locally end-to-end.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [`tshark`](https://www.wireshark.org/docs/man-pages/tshark.html) available in `PATH`
- [Docker](https://docs.docker.com/get-docker/) with Compose plugin
- `python3` (for traffic composer)
- `tcpreplay` (required to run replay/composer traffic scenarios)

## Quickstart (local E2E)

From repository root:

```bash
cd <repo-root>
```

1) Start and initialize the reference stack (Kafka/Schema Registry/Connect/ES/Postgres/Neo4j/backend/UI):

```bash
bash ./infrastructure/stack/bootstrap/reference-stack-init.sh
```

2) Start probe (separate runtime boundary):

```bash
docker compose -f docker-compose.probe.yml up -d --build
```

3) Prepare traffic composer dependencies (one-time):

```bash
python3 -m pip install -r tools/traffic/composer/requirements.txt
```

4) Validate scenario definition:

```bash
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --validate
```

5) Smoke-check service health:

```bash
./tools/traffic/validation/smoke-checks.sh
```

6) Run simulated traffic:

```bash
./tools/traffic/run.sh scenario --file tools/traffic/scenarios/one-hour-realistic.yaml --run
```

## Service URLs (local)

- Frontend UI: `http://localhost:3000`
- Backend API: `http://localhost:5090`
- Graph snapshot API: `http://localhost:5090/api/graph/devices/all?limit=200`
- Schema Registry: `http://localhost:8081`
- Kafka Connect: `http://localhost:8083`
- Elasticsearch: `http://localhost:9200`
- Neo4j Browser: `http://localhost:7474`

## Stop / Pause

Pause without removing containers:

```bash
docker compose -f docker-compose.probe.yml stop
docker compose -f docker-compose.reference-stack.yml stop
```

Stop and remove containers/networks (keep volumes):

```bash
docker compose -f docker-compose.probe.yml down
docker compose -f docker-compose.reference-stack.yml down
```

## Common issues

- `Operation not permitted` from `tcpreplay`:
  - run with elevated permissions or set Linux capabilities on `tcpreplay`.
- Graph smoke check returns `401`:
  - default smoke check uses auth headers; override with `GRAPH_AUTH_HEADER` / `GRAPH_ROLE_HEADER` if needed.
- Elasticsearch connector registration fails because index does not exist:
  - run `./infrastructure/stack/bootstrap/elasticsearch/apply-index-template.sh`, then retry connector registration.

## Development commands

Run full test suite:

```bash
dotnet test src/NetworkMonitoring.sln
```

Kafka-gated test slices:

```bash
RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter "FullyQualifiedName~KafkaSessionEventPublishIntegrationTests"
RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter "FullyQualifiedName~KafkaDeviceEventPublishIntegrationTests"
RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter "FullyQualifiedName~KafkaDeviceIngestionIntegrationTests"
```

## Documentation map

- Traffic tooling: `tools/traffic/README.md`
- Session detection quickstart: `specs/001-session-detection/quickstart.md`
- Session indexing quickstart: `specs/002-session-indexing/quickstart.md`
- Device discovery quickstart: `specs/003-device-discovery/quickstart.md`
- Device ingestion quickstart: `specs/004-device-ingestion/quickstart.md`
- Device inventory quickstart: `specs/005-device-inventory/quickstart.md`
- Device management quickstart: `specs/006-device-management/quickstart.md`
- Communication graph quickstart: `specs/007-device-communication-graph/quickstart.md`
- ADR index: `docs/adr/index.md`
- Notes: `docs/notes/`

## Repository layout

| Path | Purpose |
|------|--------|
| `src/NetworkMonitoring.Probe/` | Probe worker (capture + event publication) |
| `src/NetworkMonitoring.IntegrationConsole/` | Kafka consumer + backend ingestion forwarder |
| `src/NetworkMonitoring.Backend/` | Device Inventory + graph API |
| `src/NetworkMonitoring.Domain/` | Shared domain model + SeedWork |
| `tests/` | Unit/integration/contract tests |
| `infrastructure/stack/bootstrap/` | Stack init scripts (topics, index templates, connectors) |
| `infrastructure/stack/health/` | Service health checks |
| `infrastructure/connectors/` | Kafka Connect configs + registration scripts |
| `tools/traffic/` | Replay/composer traffic simulation toolkit |
