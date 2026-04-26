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

**Security posture**: The reference `docker-compose.reference-stack.yml` uses **PLAINTEXT** on external
listeners for local validation only. In **integration, staging, and production**, target **TLS/mTLS**
for Kafka and Registry clients per `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md` and
map certificates via `Probe` options (`KafkaSecurityProtocol`, `KafkaSslCaLocation`,
`KafkaSslCertificateLocation`, `KafkaSslKeyLocation`).

### Order of operations
1. From repo root, start the stack:
   - `docker compose -f docker-compose.reference-stack.yml up -d`
2. Wait until all three Kafka brokers report healthy (compose healthchecks), then **explicitly**
   create the topic (do not rely on broker auto-create for `sessions.detected` in documented flows):
   - `./scripts/bootstrap/kafka-topics-init.sh`  
   Defaults: **3 partitions**, **replication factor 3** (override with
   `SESSIONS_DETECTED_PARTITIONS`, `SESSIONS_DETECTED_REPLICATION_FACTOR` if the stack differs).
3. Confirm topic exists (example):
   - `docker compose -f docker-compose.reference-stack.yml exec -T kafka-1 kafka-topics --bootstrap-server kafka-1:29092 --describe --topic sessions.detected`
4. **Schema Registry**: subject **`sessions.detected-value`** is registered when the probe (or
   tests) first produces Avro values with `AutoRegisterSchemas` enabled. For governed environments,
   register the schema from `contracts/session-detected-value.avsc` via your standard tooling and
   disable auto-register in producer config as policy matures (see `contracts/session-detected-avro.md`).

### Verify the stack (smoke check)
- Run from repo root: `./scripts/stack/verify-kafka-stack.sh`  
  Confirms containers are up, Schema Registry answers on port **8081**, and `kafka-1` accepts
  clients; prints `sessions.detected` details if the topic exists.
- **After changing Kafka data paths in compose**, if brokers fail with permission or corrupt KRaft
  state, reset dev volumes once:  
  `docker compose -f docker-compose.reference-stack.yml down -v`  
  then `up -d` again and re-run `./scripts/bootstrap/kafka-topics-init.sh`.

### Listener reference (host machine)
| Service | Host ports / URL |
|--------|-------------------|
| Kafka broker 1 | `localhost:9092` |
| Kafka broker 2 | `localhost:9093` |
| Kafka broker 3 | `localhost:9094` |
| Schema Registry | `http://localhost:8081` |
| Elasticsearch (US3) | `http://localhost:9200` |
| Kafka Connect (US3) | `http://localhost:8083` |

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
- **Unknown topic or errors on produce**: run `./scripts/bootstrap/kafka-topics-init.sh` and re-check
  `--describe` on the topic.
- **Schema / deserialization errors**: ensure Registry is up and the subject
  `sessions.detected-value` matches `contracts/session-detected-value.avsc`; clear stale test subjects
  only in dev environments.
- **Connection refused to `localhost:9092`**: confirm compose is running and ports are not used by
  another stack.

## Queryable history (US3 / SC-006) — reference stack (Elasticsearch + Kafka Connect)

**Spec**: US3, **FR-017–FR-021**, **SC-006**. **ADR**:
`docs/adr/0009-elasticsearch-for-session-detection-query.md`. **Mapping**:
`contracts/elasticsearch-session-detected-mapping.md`. **Image / plugin pins (reference)**:
`research.md` (Decision 19).

**Scripts layout**: `scripts/stack/` = health smokes; `scripts/bootstrap/` = topic + index template init;
`scripts/connectors/` = Connect JSON + `register-*.sh`; `scripts/acceptance/` = SC-006 sampling (opt-in).

### Kafka-only (no ES / no Connect)
If you need **brokers + Schema Registry only** (US2), start a subset of services:
- `docker compose -f docker-compose.reference-stack.yml up -d kafka-1 kafka-2 kafka-3 schema-registry`  
  Then run `./scripts/bootstrap/kafka-topics-init.sh` and the probe as in the **Event stream (US2)** section.  
  `elasticsearch` and `kafka-connect` are optional for that path.

### Full stack (US3) — order of operations
1. `docker compose -f docker-compose.reference-stack.yml up -d`  
   First-time **Connect** can take a few minutes while `confluent-hub` installs the Elasticsearch
   sink plugin (persisted in the `connect-plugins` volume).
2. `./scripts/bootstrap/kafka-topics-init.sh` (topic **`sessions.detected`**; same as US2).
3. `./scripts/stack/verify-kafka-stack.sh` then `./scripts/stack/verify-elasticsearch-stack.sh`  
   (Connect REST must answer on **8083**; Elasticsearch on **9200**).
4. Apply index template **and** ensure the concrete index exists: `./scripts/bootstrap/elasticsearch/apply-index-template.sh`  
   (installs the composable template for `sessions-detected*` and creates `sessions-detected` if missing; Kafka Connect
   validates that this index exists before accepting `topic.to.external.resource.mapping`).
5. Register connector: `./scripts/connectors/register-elasticsearch-sink-connector.sh`  
   Config: `scripts/connectors/elasticsearch-sink-sessions-detected.json` (topic `sessions.detected` →
   index `sessions-detected` via `topic.to.external.resource.mapping` in Connect).
6. Publish events (e.g. run the probe with `EnableKafka: true` so Avro values flow through Registry).
7. **Bounded search (FR-019)**: use small page sizes; prefer `search_after` or a stable sort for pagination.

**Example** — first page (adjust `index` if yours differs; default projection index is
`sessions-detected`):

```bash
curl -sS "http://localhost:9200/sessions-detected/_search" -H "Content-Type: application/json" -d '{
  "size": 20,
  "sort": [ { "occurredAtUtc": { "order": "desc" } } ],
  "query": { "bool": {
    "filter": [
      { "range": { "occurredAtUtc": { "gte": "2026-01-01T00:00:00Z" } } },
      { "term":  { "protocol": { "value": "tcp" } } }
    ]
  } }
}'
```

- **Time range** + **term filters** on normalized `sourceIp` / `destinationIp` (keyword) and
  `protocol` match **FR-017** and **FR-006** alignment.
- Keep **`size`** bounded; for deep pagination use `search_after` with the last sort values.

### Emission-to-query lag (FR-020)
The projection is **eventually consistent**: events land in Kafka first; Connect+Elasticsearch
ingest adds latency (connector batching, `refresh_interval`, index throughput). The reference
stack does not guarantee a sub-second max delay; for **staging/production**, **document** either a
**maximum acceptable lag** to operators or the fact of **eventual consistency** (per **FR-020**), and
measure with your SLO. For a **rough local** check, note timestamps: message `occurredAtUtc` vs
document `occurredAtUtc` in `_search` hits after a short wait.

### TLS and authentication (FR-021 / security posture)
- This **reference** compose uses **PLAINTEXT** HTTP to Elasticsearch and the Connect REST API, and
  **dev relaxation** (no ES security, no Connect auth). This is **not** a production pattern.
- In **integration / production**, enable **TLS** to Elasticsearch, **secure** the Connect REST
  surface, and follow **FR-021** (organization-defined access) and the constitution (Article 8) —
  align with `plan.md` and **ADR 0009** / **ADR 0008** for a hardened deployment.

### SC-006 sampling (opt-in script)
- With data in the index:  
  `RUN_ES_INTEGRATION=1 ./scripts/acceptance/verify-sc006-elasticsearch-sampling.sh`  
  Samples returned `_source` documents for key fields that mirror `session-detected-value.avsc` (not a
  substitute for full test matrix). Record outcomes in `research.md` for releases when required.

## Out of scope (other modules)

- Session persistence in a backend database
- UI/API exposure for sessions
- Device discovery publication (separate feature `002-device-discovery`)
