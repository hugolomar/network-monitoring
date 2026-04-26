# Implementation Plan: Session Indexing

**Branch**: `002-session-indexing` | **Date**: 2026-04-20 | **Spec**: `/home/hugo/network-monitoring/specs/002-session-indexing/spec.md`  
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/002-session-indexing/spec.md` covering **FR-001–FR-005** and **SC-001**.

## Summary

This feature provides a queryable history path for `SessionDetected` events already published by
`001-session-detection`. The reference implementation uses **Elasticsearch** as a read projection fed
from Kafka via **Kafka Connect** Elasticsearch Sink, per **ADR 0009**. Kafka remains the durable log;
Elasticsearch is not a second semantic definition of "session".

## Technical Context

**Language/Version**: N/A for probe code; reference path is infrastructure and scripts  
**Primary Dependencies**: Kafka topic **`sessions.detected`**, Schema Registry subject
**`sessions.detected-value`**, **Kafka Connect**, Elasticsearch Sink connector, **Elasticsearch**  
**Storage**: Elasticsearch projection index/data stream for session detections; Kafka remains the
source event log  
**Testing**: Scripted stack checks and opt-in acceptance sampling against Elasticsearch  
**Target Platform**: Docker Compose reference stack for local validation; hardened deployment with TLS
authentication and organization access policy outside local dev  
**Constraints**: Query documents must preserve contract semantics from
`specs/001-session-detection/contracts/session-detected-value.avsc`; results must be bounded; indexing
latency must be documented

## Constitution Check

- **Boundary Contracts**: Index mapping stays aligned with the session event contract and is additive
  to the published stream contract.
- **Security Controls**: Production-class Elasticsearch and Connect endpoints require TLS and access
  controls consistent with organization policy and ADR 0009.
- **Containerized Deployables**: The reference stack uses `docker-compose.reference-stack.yml` for
  local Kafka, Registry, Elasticsearch, and Kafka Connect validation.
- **Verification Path**: SC-001 is sampled through bounded Elasticsearch queries and field-parity
  checks against the session event contract.

## Project Structure

```text
specs/002-session-indexing/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── elasticsearch-session-detected-mapping.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

Reference implementation artifacts live under:

```text
scripts/bootstrap/elasticsearch/
scripts/connectors/
scripts/stack/verify-elasticsearch-stack.sh
scripts/acceptance/verify-session-indexing-sampling.sh
docker-compose.reference-stack.yml
```

## Design Artifacts

- `contracts/elasticsearch-session-detected-mapping.md` documents Avro-to-Elasticsearch field mapping
  and query-side identity expectations.
- `quickstart.md` documents full-stack startup, index template application, connector registration,
  bounded query examples, emission-to-query lag, and security posture.
- `research.md` records the Elasticsearch + Kafka Connect decision and reference image pins.

## Next Step (Spec Kit)

Follow `specs/002-session-indexing/tasks.md` for stack, mapping, connector, documentation, and
acceptance sampling work.
