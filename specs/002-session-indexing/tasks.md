---
description: "Task list for 002-session-indexing"
---

# Tasks: Session Indexing

**Input**: Design documents from `/home/hugo/network-monitoring/specs/002-session-indexing/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**ADRs**: `docs/adr/0009-elasticsearch-for-session-detection-query.md`

**Tests**: Scripted stack verification and opt-in sampling for indexed session documents.

**Organization**: This feature indexes `SessionDetected` events emitted by `001-session-detection`.

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

---

## Phase 1: Reference stack and health checks — COMPLETE

**Purpose**: Provide Elasticsearch + Kafka Connect infrastructure for local indexing validation.

- [X] T001 Extend **`docker-compose.reference-stack.yml`** at repository root with **Elasticsearch** and **Kafka Connect** worker service(s) on the existing **`kafka-net`** network so one `up` can run the full reference stack; document the **Kafka-only** variant via explicit service list
- [X] T002 Add `scripts/stack/verify-elasticsearch-stack.sh` that checks Elasticsearch and Kafka Connect HTTP health (mirror conventions in `scripts/stack/verify-kafka-stack.sh`) and prints actionable failures

---

## Phase 2: Mapping and bootstrap — COMPLETE

**Purpose**: Define how session events become queryable documents.

- [X] T003 [P] Add `specs/002-session-indexing/contracts/elasticsearch-session-detected-mapping.md` documenting Avro -> Elasticsearch field mapping from `specs/001-session-detection/contracts/session-detected-value.avsc` (normalization and semantic parity)
- [X] T004 [P] Add Elasticsearch index template JSON under `scripts/bootstrap/elasticsearch/index-template-sessions-detected.json` plus `scripts/bootstrap/elasticsearch/apply-index-template.sh` to apply mappings in dev

---

## Phase 3: Connector registration — COMPLETE

**Purpose**: Feed Elasticsearch from the session event stream.

- [X] T005 Add connector config `scripts/connectors/elasticsearch-sink-sessions-detected.json` for the Elasticsearch Sink: consume **`sessions.detected`**, target index/data stream name, key/id strategy documented to control duplicates
- [X] T006 Add `scripts/connectors/register-elasticsearch-sink-connector.sh` that registers the connector via Connect REST API with idempotent behavior where the API allows

---

## Phase 4: Operator documentation and acceptance sampling — COMPLETE

**Purpose**: Make the indexing path reproducible and verifiable.

- [X] T007 Extend `specs/002-session-indexing/quickstart.md` with end-to-end steps: unified compose **full stack** -> topic -> ES/Connect healthy -> index template -> register connector -> probe publish -> **Elasticsearch `_search`** examples using **`size`**, sort, and `search_after` or explicit limits
- [X] T008 [P] Document **emission-to-query** latency and **eventual consistency** expectations for operators in `specs/002-session-indexing/quickstart.md` and add a short measured-note placeholder in `specs/002-session-indexing/research.md`
- [X] T009 [P] Document **TLS** and **authentication** for Elasticsearch and Connect in non-dev vs documented dev relaxation (`specs/002-session-indexing/quickstart.md`) consistent with **ADR 0009** and constitution Article 8
- [X] T010 Add `scripts/acceptance/verify-session-indexing-sampling.sh` that runs a **bounded** query, samples hits, and fails if required fields/semantics diverge from `session-detected-value.avsc` (gated with `RUN_ES_INTEGRATION=1`)
- [X] T011 [P] Pin **Elasticsearch** and **Kafka Connect** image versions and connector plugin strategy in `specs/002-session-indexing/research.md` and cross-reference in `specs/002-session-indexing/quickstart.md`

**Checkpoint**: Operators can bring up the **unified** reference compose (Kafka + Registry + ES + Connect), index session detections, and reproduce **SC-001** using the documented query + script path; **Kafka-only** remains documented for lighter stream runs.

---

## Dependencies & Execution Order

| Phase | Depends on |
|-------|------------|
| 1 | `001-session-detection` event stream contract and reference Kafka stack |
| 2 | 1 |
| 3 | 1–2 |
| 4 | 1–3 |

---

## Parallel Opportunities

- T003 and T004 after Phase 1
- T008, T009, and T011 after quickstart structure exists

---

## Implementation Strategy

1. Bring up `docker-compose.reference-stack.yml` and confirm Kafka + Registry.
2. Confirm Elasticsearch + Connect health.
3. Apply the index template and register the connector.
4. Publish session events through `001-session-detection`.
5. Run bounded query examples and opt-in sampling.

---

## Notes

- Kafka topic **`sessions.detected`** and subject **`sessions.detected-value`** remain owned by the session event contract.
- Mapping changes that alter query semantics require explicit compatibility review.
- Path to this file: `/home/hugo/network-monitoring/specs/002-session-indexing/tasks.md`
- **Task count**: T001–T011 **complete** (**11**). **Total defined: 11.**
