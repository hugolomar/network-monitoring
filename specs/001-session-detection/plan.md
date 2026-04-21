# Implementation Plan: Probe Session Detection Visibility

**Branch**: `001-session-detection` | **Date**: 2026-04-20 | **Spec**: `/home/hugo/network-monitoring/specs/001-session-detection/spec.md`
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/001-session-detection/spec.md`
covering **US1–US3**, **FR-001–FR-021**, **SC-001–SC-006**. This merge **preserves** the prior plan
text for probe, console, Kafka/Avro, Registry, compose/scripts, tests, **SC-005**, and ADRs **0006–
0008**; it **adds** symmetric coverage for **US3** and **ADR 0009** only in the US3 scope.

**Note**: This file is produced by the `/speckit.plan` workflow. See `.specify/templates/plan-template.md` for structure. **`tasks.md` is NOT generated here** — run **`/speckit.tasks`** next for executable task breakdown (Spec Kit handoff).

## Summary

The feature provides **three capabilities** over the **same** session detection semantics (`spec.md`):

1. **US1**: **Operator-visible** structured records (**console** / JSONL) for live validation
   (**SC-001–SC-004**).
2. **US2**: **Optional publication** of validated outcomes to the platform **asynchronous event stream**
   (reference: **Kafka** + **Schema Registry**, Avro per **ADR 0006**, topic **`sessions.detected`**,
   **SC-005**).
3. **US3**: **Queryable** past detections via a **documented search** path (**FR-017–FR-021**,
   **SC-006**). Reference implementation: **Elasticsearch** as a **read projection** fed from Kafka
   (**Kafka Connect** Elasticsearch Sink as reference ingest), per **ADR 0009**. Kafka remains the
   **durable log**; Elasticsearch is **not** a second semantic definition of “session”.

**Shared architecture** (all stories): clean/hexagonal boundaries — `ITrafficProvider` for capture,
`IMessagePublisher` for outputs; **Kafka/Avro** adapter alongside **ConsolePublisher** without changing
domain or use-case rules. Serialization for the bus follows **`session-detected-value.avsc`** with
Schema Registry subject **`sessions.detected-value`**; default topic **`sessions.detected`**. Cluster
metadata uses **KRaft** (no ZooKeeper) per **ADR 0007**. **mTLS** toward Kafka (and Registry where
applicable) in integration/staging/production per **ADR 0008**; local dev may use documented relaxed
transport only (**FR-016**).

Architecture decision records (binding for implementation planning): **`docs/adr/0006`**, **`0007`**,
**`0008`**; **`docs/adr/0009`** applies to **US3** (query projection / Elasticsearch / Connect).

**Delivery note (neutral)**: The repository currently implements and verifies **US1** and **US2**;
**US3** is the following engineering increment (infra + verification per plan/tasks).

## Planned outcomes by user story *(symmetric plan view)*

### US1 — Observe captured sessions live

- **Intent**: Operators validate capture and stable session shape without downstream systems.
- **Primary pieces**: `NetworkMonitoring.Probe` — capture adapter (`ITrafficProvider` / tshark path),
  validation, **ConsolePublisher**, contracts for console JSONL as applicable.
- **Verification**: **SC-001–SC-004**; xUnit unit/integration tests for capture-to-console.

### US2 — Feed the platform session event stream

- **Intent**: Same validated detections on **Kafka** for platform consumers; **FR-013–FR-016**;
  payload **FR-015** → **`session-detected-value.avsc`** / Registry subject **`sessions.detected-value`**.
- **Primary pieces**: Infrastructure **Kafka + Avro** publisher; **explicit** topic **`sessions.detected`**;
  `docker-compose.reference-stack.yml`, `scripts/kafka-topics-init.sh`, Registry; **mTLS** story non-dev (**ADR 0008**).
- **Verification**: **SC-005**; opt-in integration test `KafkaSessionPublishIntegrationTests` with
  `RUN_KAFKA_INTEGRATION=1`; manual steps in `quickstart.md`.

### US3 — Query past session detections

- **Intent**: **FR-017–FR-021** — documented filters (time range, normalized addresses, ports when
  applicable, protocol), **bounded** results (**FR-019**), documented emission-to-query expectations
  (**FR-020**), org access (**FR-021**); results **FR-018**-aligned with session contract.
- **Primary pieces (reference)**: **Kafka Connect** (Elasticsearch **Sink**) from **`sessions.detected`**
  (or derived topic) → **Elasticsearch** indices/data streams; mapping from Avro value; optional
  contract note under `contracts/` for ES fields; compose/scripts for dev when added.
- **Verification**: **SC-006** — sample query hits vs contract semantics (documented when stack exists).
- **ADR scope**: **ADR 0009** records the Elasticsearch decision; probe code stays unaware of ES.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: .NET Worker host; shared SeedWork domain; **tshark** CLI; **Confluent.Kafka**
(or equivalent) + **Confluent.SchemaRegistry** + **Avro** serializers for producers; existing xUnit
test stack; **US3**: **Kafka Connect** + **Elasticsearch** for reference ingest/query validation
(versions pinned in implementation / `research.md`)  
**Storage**: **Apache Kafka** (reference: **3 brokers**, **KRaft**) + **Confluent-compatible Schema
Registry** for emitted **SessionDetected** events; default topic **`sessions.detected`**; value subject
**`sessions.detected-value`** (see `contracts/session-detected-avro.md`). **US3**: **Elasticsearch** as
**query projection** (ADR **0009**), not authoritative log.  
**Testing**: xUnit — unit tests for mapping/serialization; integration tests for console path;
integration or container-based tests for **publish-to-Kafka** when compose stack is available; **US3**:
scripted or container checks for **index + search** sampling (**SC-006**) when ES stack is available  
**Target Platform**: Linux host/container with capture capability; Docker Compose (or equivalent)
for **Kafka + Registry** in dev/integration; **US3**: Elasticsearch + Connect added to the **same**
reference **`docker-compose.reference-stack.yml`** (Kafka-only still via explicit service list)  
**Project Type**: single deployable probe module (`src/NetworkMonitoring.Probe`) with folder layering  
**Performance Goals**: Near real-time console visibility; event stream suitable for **high-volume**
session telemetry (Avro choice per ADR 0006); **US3**: interactive search with **bounded** pages
(**FR-019**)  
**Constraints**: SeedWork immutability (constitution); invalid observations must not stop stream;
session identity for deduplication and **Kafka record key** must match spec (same deterministic
fields); **TLS minimum**, **mTLS** for non-dev per ADR 0008; Kafka cluster **KRaft only** per ADR 0007;
contract evolution **backward-compatible** unless breaking change declared; **US3**: ES document
semantics must stay aligned with session contract (**FR-018**)  
**Operational standard (topics)**: Topic **`sessions.detected`** MUST be **provisioned explicitly**
(partitions, replication factor, documented in quickstart/scripts). The documented reference path MUST
**not** depend on **broker auto-create** as the primary mechanism; production/staging SHOULD mirror
this with **IaC or approved admin tooling**.  
**Scale/Scope**: One probe instance; session detection with **console**, **stream**, and **planned query
projection**; no separate product UI API required in this slice unless added later

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: `Session` remains shared-domain entity inheriting `Entity` (no
  surrogate id from probe).
- **SeedWork Immutability**: Only constitution-allowed edits under `SeedWork/`.
- **Boundary Contracts**: Console JSONL (`console-record-schema.md`); Kafka Avro value
  (`session-detected-value.avsc`, `session-detected-avro.md`); capture/output ports
  (`probe-capture-contract.md`, `probe-output-contract.md`). Topic and subject names are stable
  contracts per Article 6–7. **US3**: Elasticsearch **projection** fields MUST align with the same
  session meaning (**FR-018**); optional additive mapping doc under `contracts/`.
- **Security Controls**: **Article 8** — TLS/mTLS for service-to-service; probe-to-Kafka uses
  **encrypted** transport; **mTLS** in integration/staging/production (ADR 0008). Dev-only relaxation
  must be **documented** (spec FR-016). **US3**: **TLS** and **authenticated** access to Elasticsearch
  and Connect in production-class environments; dev relaxation documented if used.
- **Containerized Deployables**: Probe Dockerfile maintained; reference **`docker-compose.reference-stack.yml`**
  (Kafka + Registry; **US3**: + Elasticsearch + Connect when implemented), separate from
  `docker-compose.probe.yml` where applicable.
- **Incremental Compatibility**: Maintainer confirmation if changing published contracts affects
  consumers; **US3** mapping changes that alter query semantics require the same discipline.
- **Verification Path**: Unit/integration tests + **SC-005** (consume configured destination, verify
  contract). **US3**: **SC-006** (sample query results, verify contract alignment).

**Gate status (pre-design)**: PASS — scope extends emission destinations; no constitution violation
if mTLS and contracts are honored.

**Gate status (post-design update in this plan)**: PASS — security updated from “console only” to
**stream transport + authentication posture** aligned with ADR 0008; **US3** adds **ES + Connect**
under ADR 0009 without changing US1/US2 semantics.

## Project Structure

### Documentation (this feature)

```text
specs/001-session-detection/
├── plan.md              # This file (/speckit.plan)
├── research.md          # Phase 0
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md             # /speckit.tasks (not created by plan)
```

### Source code (repository root)

```text
src/
├── NetworkMonitoring.Domain/
│   └── Shared/ ...
└── NetworkMonitoring.Probe/
    ├── Application/
    ├── Infrastructure/   # TsharkTrafficProvider, ConsolePublisher, Kafka + Avro publisher
    ├── Host/
    └── NetworkMonitoring.Probe.csproj

tests/
├── NetworkMonitoring.Probe.UnitTests/
└── NetworkMonitoring.Probe.IntegrationTests/
```

**Structure decision**: Add Kafka producer adapter under `Infrastructure/Publishing/` (or
`Infrastructure/Messaging/`), configuration under `Probe` options extension (bootstrap, registry URL,
topic, SSL/mTLS material paths or env), host DI registration conditional on feature flags.

**US3**: Connector configs, Elasticsearch index templates, verification scripts — locations per
`/speckit.tasks` (not in probe core unless a dedicated query API is added later).

**Dependency inversion**:

- `IMessagePublisher` gains or shares responsibility for `PublishSessionDetected` to Kafka when
  enabled (composite publisher or separate registration strategy — detail in `/speckit.tasks`).
- Use case remains unaware of Avro/Registry; adapter maps `Session` → Avro record per `.avsc`.
- **US3**: Ingest/search **outside** probe process; no ES client required in `Session` use case.

## Phase 0 / Research

Consolidated in `research.md` (updated 2026-04-20): operational choices for **Kafka image/stack**
(Apache vs Confluent Platform community images), **listener** layout (internal vs external clients),
**dev PLAINTEXT** vs **TLS** bridge, and **CI** strategy for compose-based tests. All prior Phase 0
decisions for capture/console remain valid.

**US3 additive entries** in `research.md`: Elasticsearch + Kafka Connect reference path, dev stack
pinning, emission-to-query lag assumptions — **without removing** prior decisions.

## Phase 1 / Design artifacts

- **`data-model.md`**: Extended with **event stream** emission notes (logical parity console ↔ Kafka
  value); **US3** short note on **read projection** (same session semantics, ES not a new domain entity).
- **`contracts/`**: Existing Avro + console contracts are authoritative; no breaking rename without
  explicit version bump. **US3**: optional ES field-mapping document (additive).
- **`quickstart.md`**: **Kafka + Registry** bring-up, **explicit** `sessions.detected` provisioning
  (script or documented admin commands), Registry subject setup, and probe configuration for stream
  publication; **US3** section stub for ES + Connect + **SC-006** when implemented.

## Agent context

Run after plan update: `.specify/scripts/bash/update-agent-context.sh cursor-agent` (adds Kafka,
Registry, Avro, mTLS; **US3**: Elasticsearch, Kafka Connect when reflected in plan).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

---

## Next step (Spec Kit)

Follow **`specs/001-session-detection/tasks.md`** for implementation phases: infra (KRaft 3 brokers +
Registry), **explicit topic** `sessions.detected` + Registry subject, mTLS/cert story for non-dev,
probe Kafka publisher + config, and E2E verification for SC-005.

**US3 / ADR 0009**: extend **`/speckit.tasks`** with Elasticsearch, Kafka Connect, index/data-stream
bootstrap, connector configuration, secured query path, and **SC-006** — **merge** with existing
tasks so completed US1/US2 items stay marked done.
