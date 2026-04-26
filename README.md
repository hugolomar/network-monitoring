# Network monitoring

Hands-on, event-driven network monitoring: a passive probe captures traffic (via **tshark**), derives **sessions**, and can emit **structured events** to the console and/or **Apache Kafka** (Avro + Schema Registry). This repository includes specs, ADRs, reference Docker Compose (Kafka; optional Elasticsearch and Kafka Connect for queryable history), and **shell helpers** under `scripts/` grouped by role — see **Repository layout** below.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [`tshark`](https://www.wireshark.org/docs/man-pages/tshark.html) on `PATH` (for live capture)
- [Docker](https://docs.docker.com/get-docker/) (optional, for the Kafka stack)

## Quick start — probe only

From the repository root:

```bash
dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj
```

Configure the `Probe` section in `src/NetworkMonitoring.Probe/appsettings.json` (interface name, capture filter, `EnableConsole` / `EnableKafka`, etc.).

## Tests

```bash
dotnet test src/NetworkMonitoring.Probe.sln
```

Optional Kafka end-to-end test (requires the Compose stack and topic init; see **More detail** below):

```bash
RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.Probe.sln --filter "FullyQualifiedName~KafkaSessionPublishIntegrationTests"
```

## Kafka (local reference stack)

```bash
docker compose -f docker-compose.reference-stack.yml up -d
./scripts/bootstrap/kafka-topics-init.sh
./scripts/stack/verify-kafka-stack.sh
```

The full path (Schema Registry, optional Elasticsearch + Kafka Connect, connector registration) is in the quickstart.

## More detail

- Feature flow and operator steps: [`specs/001-session-detection/quickstart.md`](specs/001-session-detection/quickstart.md)
- Architecture decisions: [`docs/adr/`](docs/adr/)

## Repository layout

| Path | Purpose |
|------|--------|
| `src/NetworkMonitoring.Probe/` | Probe worker (Application / Infrastructure / Host) |
| `src/NetworkMonitoring.Domain/` | Shared domain (SeedWork + entities/value objects) |
| `tests/` | Unit and integration tests |
| `specs/` | Feature specifications and contracts |
| `scripts/stack/` | Smoke checks: Kafka/Registry; Elasticsearch/Connect |
| `scripts/bootstrap/` | Idempotent init: topics, index templates |
| `scripts/connectors/` | Kafka Connect JSON + `register-*.sh` |
| `scripts/acceptance/` | Spec checks (e.g. SC-006 sampling, `RUN_ES_INTEGRATION=1`) |
