# Quickstart: Probe Session Detection Visibility

## Goal
Run the probe in local validation mode and verify that session entities are emitted to console as
structured records. Optionally publish the same validated detections to Kafka.

## Prerequisites
- .NET 10 SDK installed
- `tshark` installed and available in PATH
- Network interface or capture source available for test traffic
- Docker engine with compose plugin (for containerized execution path and Kafka validation)

## Configuration (session scope)
- Application settings section `Probe` (see `src/NetworkMonitoring.Probe/appsettings.json`):
  - `TSharkPath`: capture executable (default `tshark`).
  - `InterfaceName`: capture interface (default `eth0`; adjust to your environment).
  - `CaptureFilter`: optional BPF-style filter string.
  - `SessionDeduplicationWindowMinutes`: sliding window for suppressing duplicate `SessionDetected`
    emissions for the same session identity (default `10`; use `0` to disable).

## Steps
1. Restore and build solution/projects.
2. Start the probe module in console output mode.
3. Generate or observe representative network traffic.
4. Confirm console emits `SessionDetected` records.
5. Introduce malformed/partial sample input and verify processing continues.

## Expected Outcomes
- At least one valid session record is printed.
- Repeated similar traffic produces stable payload structures.
- Malformed observations are dropped with diagnostics, without stopping the probe.
- Invalid observations report explicit validation errors in logs and do not rely on exception control flow for normal handling.

## Validation Commands (example flow)
- Build and test:
  - `dotnet test src/NetworkMonitoring.sln`
- Start probe:
  - `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Optional startup smoke check:
  - `timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Build container image:
  - `docker build -f src/NetworkMonitoring.Probe/Dockerfile -t network-monitoring-probe:local .`
- Run probe container (host networking + capture capabilities):
  - `docker compose -f docker-compose.probe.yml up --build`

## Event stream (Kafka + Schema Registry) — reference stack (US2 / SC-005)

**Security posture**: The reference `docker-compose.reference-stack.yml` uses PLAINTEXT on external
listeners for local validation only. In integration, staging, and production, target TLS/mTLS for Kafka
and Registry clients per `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md` and map certificates
via `Probe` options (`KafkaSecurityProtocol`, `KafkaSslCaLocation`, `KafkaSslCertificateLocation`,
`KafkaSslKeyLocation`).

### Order of operations
1. From repo root, start the Kafka services:
   - `docker compose -f docker-compose.reference-stack.yml up -d kafka-1 kafka-2 kafka-3 schema-registry`
2. Wait until all three Kafka brokers report healthy, then explicitly create the topic:
   - `./scripts/bootstrap/kafka-topics-init.sh`
   Defaults: 3 partitions, replication factor 3.
3. Confirm topic exists:
   - `docker compose -f docker-compose.reference-stack.yml exec -T kafka-1 kafka-topics --bootstrap-server kafka-1:29092 --describe --topic sessions.detected`
4. Schema Registry subject `sessions.detected-value` is registered when the probe or tests first produce
   Avro values with `AutoRegisterSchemas` enabled. Governed environments should register
   `contracts/session-detected-value.avsc` through approved tooling and disable auto-register when policy requires it.

### Verify the stack
- Run from repo root: `./scripts/stack/verify-kafka-stack.sh`
- After changing Kafka data paths in compose, if brokers fail with permission or corrupt KRaft state,
  reset dev volumes once: `docker compose -f docker-compose.reference-stack.yml down -v`.

### Listener reference (host machine)
| Service | Host ports / URL |
|--------|-------------------|
| Kafka broker 1 | `localhost:9092` |
| Kafka broker 2 | `localhost:9093` |
| Kafka broker 3 | `localhost:9094` |
| Schema Registry | `http://localhost:8081` |

Bootstrap for clients on the host: `localhost:9092,localhost:9093,localhost:9094`.

### Probe configuration (`Probe` section)
- `EnableKafka`: `true` to publish sessions to Kafka.
- `EnableConsole`: `true` / `false` — can run console only, Kafka only, or both.
- `KafkaBootstrapServers`, `SchemaRegistryUrl`, `KafkaSessionTopic` (`sessions.detected` default).
- Optional TLS: `KafkaSecurityProtocol` and SSL file paths as above.

Example: run with Kafka enabled:
- `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj -- --environment Development`

### Validate publication
- Automated (opt-in):
  - `RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter FullyQualifiedName~KafkaSessionEventPublishIntegrationTests`
- Manual: consume `sessions.detected`, deserialize Avro using Registry, and confirm fields match
  `session-detected-value.avsc` (SC-005).

### Troubleshooting
- Unknown topic or errors on produce: run `./scripts/bootstrap/kafka-topics-init.sh` and re-check the topic.
- Schema / deserialization errors: ensure Registry is up and the subject `sessions.detected-value` matches the Avro contract.
- Connection refused to `localhost:9092`: confirm compose is running and ports are not used by another stack.

## Out of scope (other modules)

- Session indexing and historical query projection
- Session persistence in a backend database
- UI/API exposure for sessions
- Device discovery publication (separate feature `003-device-discovery`)
