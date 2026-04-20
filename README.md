# Network monitoring

Hands-on, event-driven network monitoring: a passive probe captures traffic (via **tshark**), derives **sessions**, and can emit **structured events** to the console and/or **Apache Kafka** (Avro + Schema Registry). The repo also holds specs, ADRs, and reference Docker Compose for local Kafka.

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
docker compose -f docker-compose.kafka.yml up -d
./scripts/kafka-topics-init.sh
./scripts/verify-kafka-stack.sh
```

## More detail

- Feature flow and operators’ steps: [`specs/001-session-detection/quickstart.md`](specs/001-session-detection/quickstart.md)
- Architecture decisions: [`docs/adr/`](docs/adr/)

## Repository layout

| Path | Purpose |
|------|--------|
| `src/NetworkMonitoring.Probe/` | Probe worker (Application / Infrastructure / Host) |
| `src/NetworkMonitoring.Domain/` | Shared domain (SeedWork + entities/value objects) |
| `tests/` | Unit and integration tests |
| `specs/` | Feature specifications and contracts |
| `scripts/` | Kafka topic init and stack smoke checks |
