# Quickstart: Probe Session Detection Visibility

## Goal
Run the probe in local validation mode and verify that session entities are emitted to
console as structured records.

## Prerequisites
- .NET 10 SDK installed
- `tshark` installed and available in PATH
- Network interface or capture source available for test traffic
- Docker engine with compose plugin (for containerized execution path)

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
- Invalid observations report explicit validation errors in logs and do not rely on exception
  control flow for normal handling.

## Validation Commands (example flow)
- Build and test:
  - `dotnet test src/NetworkMonitoring.Probe.sln`
- Start probe:
  - `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Optional startup smoke check:
  - `timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Build container image:
  - `docker build -f src/NetworkMonitoring.Probe/Dockerfile -t network-monitoring-probe:local .`
- Run probe container (host networking + capture capabilities):
  - `docker compose -f docker-compose.probe.yml up --build`

## Event stream (Kafka + Schema Registry) — reference stack (US2 / SC-005)

**Security posture**: The reference `docker-compose.kafka.yml` uses **PLAINTEXT** on external
listeners for local validation only. In **integration, staging, and production**, target **TLS/mTLS**
for Kafka and Registry clients per `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md` and
map certificates via `Probe` options (`KafkaSecurityProtocol`, `KafkaSslCaLocation`,
`KafkaSslCertificateLocation`, `KafkaSslKeyLocation`).

### Order of operations
1. From repo root, start the stack:
   - `docker compose -f docker-compose.kafka.yml up -d`
2. Wait until all three Kafka brokers report healthy (compose healthchecks), then **explicitly**
   create the topic (do not rely on broker auto-create for `sessions.detected` in documented flows):
   - `./scripts/kafka-topics-init.sh`  
   Defaults: **3 partitions**, **replication factor 3** (override with
   `SESSIONS_DETECTED_PARTITIONS`, `SESSIONS_DETECTED_REPLICATION_FACTOR` if the stack differs).
3. Confirm topic exists (example):
   - `docker compose -f docker-compose.kafka.yml exec -T kafka-1 kafka-topics --bootstrap-server kafka-1:29092 --describe --topic sessions.detected`
4. **Schema Registry**: subject **`sessions.detected-value`** is registered when the probe (or
   tests) first produces Avro values with `AutoRegisterSchemas` enabled. For governed environments,
   register the schema from `contracts/session-detected-value.avsc` via your standard tooling and
   disable auto-register in producer config as policy matures (see `contracts/session-detected-avro.md`).

### Verify the stack (smoke check)
- Run from repo root: `./scripts/verify-kafka-stack.sh`  
  Confirms containers are up, Schema Registry answers on port **8081**, and `kafka-1` accepts
  clients; prints `sessions.detected` details if the topic exists.
- **After changing Kafka data paths in compose**, if brokers fail with permission or corrupt KRaft
  state, reset dev volumes once:  
  `docker compose -f docker-compose.kafka.yml down -v`  
  then `up -d` again and re-run `./scripts/kafka-topics-init.sh`.

### Listener reference (host machine)
| Service | Host ports / URL |
|--------|-------------------|
| Kafka broker 1 | `localhost:9092` |
| Kafka broker 2 | `localhost:9093` |
| Kafka broker 3 | `localhost:9094` |
| Schema Registry | `http://localhost:8081` |

Bootstrap for clients on the host: `localhost:9092,localhost:9093,localhost:9094` (matches
`Probe:KafkaBootstrapServers` default in `appsettings.json`).

### Probe configuration (`Probe` section)
- `EnableKafka`: `true` to publish sessions to Kafka.
- `EnableConsole`: `true` / `false` — can run **console only**, **Kafka only**, or **both**
  (composite publisher).
- `KafkaBootstrapServers`, `SchemaRegistryUrl`, `KafkaSessionTopic` (`sessions.detected` default).
- Optional TLS: `KafkaSecurityProtocol` and SSL file paths as above.

Example: run with Kafka enabled (adjust interface and capture as needed):
- `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj -- --environment Development`  
  (or set env vars / user secrets overriding `Probe:EnableKafka`, etc.)

### Validate publication
- **Automated (opt-in)**: with the stack up and topic created, run:
  - `RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.Probe.sln --filter FullyQualifiedName~KafkaSessionPublishIntegrationTests`  
  Override `KAFKA_BOOTSTRAP_SERVERS`, `SCHEMA_REGISTRY_URL`, or `KAFKA_SESSION_TOPIC` if needed.
- **Manual**: consume `sessions.detected` with your preferred tool (e.g. `kcat`, `kafka-console-consumer`,
  or a small Confluent consumer), deserialize Avro using Registry, and confirm fields match
  `session-detected-value.avsc` (SC-005).

### Troubleshooting
- **Unknown topic or errors on produce**: run `./scripts/kafka-topics-init.sh` and re-check
  `--describe` on the topic.
- **Schema / deserialization errors**: ensure Registry is up and the subject
  `sessions.detected-value` matches `contracts/session-detected-value.avsc`; clear stale test subjects
  only in dev environments.
- **Connection refused to `localhost:9092`**: confirm compose is running and ports are not used by
  another stack.

## Out of scope (other modules)

- Session persistence in a backend database
- UI/API exposure for sessions
- Device discovery publication (separate feature `002-device-discovery`)
