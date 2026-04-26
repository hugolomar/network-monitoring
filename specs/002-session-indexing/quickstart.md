# Quickstart: Session Indexing

## Goal

Index `SessionDetected` events from Kafka into Elasticsearch and verify that past detections are
queryable with bounded filters.

## Prerequisites

- Session publication from `001-session-detection` available on Kafka topic `sessions.detected`.
- Docker engine with compose plugin.
- `curl`, `bash`, and `python3` for reference scripts.

## Reference Stack

**Spec**: `002-session-indexing`, **FR-001–FR-005**, **SC-001**. **ADR**:
`docs/adr/0009-elasticsearch-for-session-detection-query.md`. **Mapping**:
`contracts/elasticsearch-session-detected-mapping.md`. **Image / plugin pins (reference)**:
`research.md` (Decision 2).

**Scripts layout**: `scripts/stack/` = health smokes; `scripts/bootstrap/` = topic + index template init;
`scripts/connectors/` = Connect JSON + `register-*.sh`; `scripts/acceptance/` = sampling (opt-in).

### Host ports / URLs

| Service | Host ports / URL |
|--------|-------------------|
| Kafka broker 1 | `localhost:9092` |
| Kafka broker 2 | `localhost:9093` |
| Kafka broker 3 | `localhost:9094` |
| Schema Registry | `http://localhost:8081` |
| Elasticsearch | `http://localhost:9200` |
| Kafka Connect | `http://localhost:8083` |

### Full stack — order of operations

1. `docker compose -f docker-compose.reference-stack.yml up -d`  
   First-time **Connect** can take a few minutes while `confluent-hub` installs the Elasticsearch
   sink plugin (persisted in the `connect-plugins` volume).
2. `./scripts/bootstrap/kafka-topics-init.sh` (topic **`sessions.detected`**).
3. `./scripts/stack/verify-kafka-stack.sh` then `./scripts/stack/verify-elasticsearch-stack.sh`  
   (Connect REST must answer on **8083**; Elasticsearch on **9200**).
4. Apply index template **and** ensure the concrete index exists: `./scripts/bootstrap/elasticsearch/apply-index-template.sh`  
   (installs the composable template for `sessions-detected*` and creates `sessions-detected` if missing; Kafka Connect
   validates that this index exists before accepting `topic.to.external.resource.mapping`).
5. Register connector: `./scripts/connectors/register-elasticsearch-sink-connector.sh`  
   Config: `scripts/connectors/elasticsearch-sink-sessions-detected.json` (topic `sessions.detected` ->
   index `sessions-detected` via `topic.to.external.resource.mapping` in Connect).
6. Publish events (e.g. run the probe with `EnableKafka: true` so Avro values flow through Registry).
7. **Bounded search (FR-003)**: use small page sizes; prefer `search_after` or a stable sort for pagination.

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
  `protocol` match **FR-001** and event-contract normalization.
- Keep **`size`** bounded; for deep pagination use `search_after` with the last sort values.

### Emission-to-query lag (FR-004)

The projection is **eventually consistent**: events land in Kafka first; Connect+Elasticsearch
ingest adds latency (connector batching, `refresh_interval`, index throughput). The reference
stack does not guarantee a sub-second max delay; for **staging/production**, **document** either a
**maximum acceptable lag** to operators or the fact of **eventual consistency** (per **FR-004**), and
measure with your SLO. For a **rough local** check, note timestamps: message `occurredAtUtc` vs
document `occurredAtUtc` in `_search` hits after a short wait.

### TLS and authentication (FR-005 / security posture)

- This **reference** compose uses **PLAINTEXT** HTTP to Elasticsearch and the Connect REST API, and
  **dev relaxation** (no ES security, no Connect auth). This is **not** a production pattern.
- In **integration / production**, enable **TLS** to Elasticsearch, **secure** the Connect REST
  surface, and follow **FR-005** (organization-defined access) and the constitution (Article 8) —
  align with `plan.md`, **ADR 0009**, and **ADR 0008** for a hardened deployment.

### Sampling (opt-in script)

- With data in the index:  
  `RUN_ES_INTEGRATION=1 ./scripts/acceptance/verify-session-indexing-sampling.sh`  
  Samples returned `_source` documents for key fields that mirror `session-detected-value.avsc` (not a
  substitute for full test matrix). Record outcomes in `research.md` for releases when required.

## Out of scope

- Session capture and Kafka publication behavior (owned by `001-session-detection`)
- Backend relational persistence
- UI/API exposure for sessions
