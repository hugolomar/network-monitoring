# Research: Session Indexing

## Decision 1: Queryable session history — store and ingest

- **Decision**: Use **Elasticsearch** as the **reference** system for indexing and querying past
  session detections, with **Kafka Connect** (Elasticsearch **Sink** connector) as the reference ingest
  path from **`sessions.detected`**, per `docs/adr/0009-elasticsearch-for-session-detection-query.md`.
- **Rationale**: Search-first filters (time range, normalized addresses, ports, protocol); mature
  Connect sink; satisfies the query requirements without using Kafka alone as the operator history UI.
- **Alternatives considered**: RDBMS as primary interactive search store — deferred per ADR 0009;
  Kafka-only replay for “query” — rejected for this increment.

## Decision 2: Reference dev stack (Elasticsearch + Connect)

- **Decision**: **`docker-compose.reference-stack.yml`** includes **Elasticsearch** and **Kafka Connect**
  on **`kafka-net`**, plus a **Schema Registry healthcheck** so Connect can depend on a ready Registry.
  **Image pins (reference)**:
  - **Elasticsearch**: `docker.elastic.co/elasticsearch/elasticsearch:8.11.4` (single-node, `xpack.security.enabled=false` for local dev only).
  - **Kafka Connect**: `confluentinc/cp-kafka-connect:7.6.1` (aligns with the broker line used by the session event stream).
  - **Elasticsearch Sink plugin**: `confluentinc/kafka-connect-elasticsearch:14.1.6` (installed on first `connect` container start via `confluent-hub`, cached in `connect-plugins` volume; version must exist on Confluent Hub — see hub API).
- **Rationale**: Same single-manifest `up` for full stack; reproduces sampling with scripts in
  `scripts/` (see `stack/`, `bootstrap/`, `connectors/`, `acceptance/`). Kafka-only subset documented in the session detection quickstart.
- **Alternatives considered**: Pre-baked Connect image with baked-in plugin — deferred; hub install +
  named volume keeps compose file readable.

## SC-001 (Elasticsearch query sampling)

- **Intent**: With the indexing stack up, connector running, and documents in **`sessions-detected`**
  (from topic **`sessions.detected`**), sample `_search` hits and verify **required** business fields
  present and consistent with **`session-detected-value.avsc`** / `elasticsearch-session-detected-mapping.md`.
- **Automation (opt-in)**: `RUN_ES_INTEGRATION=1 ./scripts/acceptance/verify-session-indexing-sampling.sh` (see
  `quickstart.md`). Skips when unset so CI without ES does not fail.
- **Recorded outcome (manual)**: Operators run the script (or equivalent bounded queries) after traffic
  + probe publication; attach evidence to release notes when required. **Emission-to-query latency** is
  **environment-specific** — document measured notes here when available (FR-004).
