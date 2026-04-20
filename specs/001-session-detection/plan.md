# Implementation Plan: Probe Session Detection Visibility

**Branch**: `001-session-detection` | **Date**: 2026-04-20 | **Spec**: `/home/hugo/network-monitoring/specs/001-session-detection/spec.md`
**Input**: Feature specification from `/specs/001-session-detection/spec.md` (regenerated plan: operator-visible output **and** session event stream publication per US2, FR-013–FR-016, SC-005).

**Note**: This file is produced by the `/speckit.plan` workflow. See `.specify/templates/plan-template.md` for structure. **`tasks.md` is NOT generated here** — run **`/speckit.tasks`** next for executable task breakdown (Spec Kit handoff).

## Summary

Deliver probe session detection with **two observable outputs**: (1) **operator-visible** structured
records (console) for live validation (US1), and (2) **optional publication** of the same validated
session outcomes to the platform **asynchronous event stream** (Apache Kafka) (US2). Clean
architecture is preserved: `ITrafficProvider` for capture, `IMessagePublisher` for outputs; add a
**Kafka/Avro** adapter alongside **ConsolePublisher** without changing domain or use-case rules.
Serialization for the bus follows **`session-detected-value.avsc`** with Schema Registry subject
**`sessions.detected-value`**; default topic **`sessions.detected`**. Cluster metadata uses **KRaft**
(no ZooKeeper) per ADR 0007. **mTLS** toward Kafka (and Registry where applicable) in
integration/staging/production per ADR 0008; local dev may use documented relaxed transport only.

Architecture decision records (binding for implementation planning): **`docs/adr/0006`**, **`0007`**,
**`0008`**.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: .NET Worker host; shared SeedWork domain; **tshark** CLI; **Confluent.Kafka**
(or equivalent) + **Confluent.SchemaRegistry** + **Avro** serializers for producers; existing xUnit
test stack  
**Storage**: N/A for session persistence in this slice; **Apache Kafka** (reference: **3 brokers**,
**KRaft**) + **Confluent-compatible Schema Registry** for emitted **SessionDetected** events; default
topic **`sessions.detected`**; value subject **`sessions.detected-value`** (see
`contracts/session-detected-avro.md`)  
**Testing**: xUnit — unit tests for mapping/serialization; integration tests for console path; add
integration or container-based tests for **publish-to-Kafka** path when compose stack is available  
**Target Platform**: Linux host/container with capture capability; Docker Compose (or equivalent)
for **Kafka + Registry** in dev/integration  
**Project Type**: single deployable probe module (`src/NetworkMonitoring.Probe`) with folder layering  
**Performance Goals**: Near real-time console visibility; event stream suitable for **high-volume**
session telemetry (Avro choice per ADR 0006)  
**Constraints**: SeedWork immutability (constitution); invalid observations must not stop stream;
session identity for deduplication and **Kafka record key** must match spec (same deterministic
fields); **TLS minimum**, **mTLS** for non-dev per ADR 0008; Kafka cluster **KRaft only** per ADR 0007;
contract evolution **backward-compatible** unless breaking change declared  
**Operational standard (topics)**: Topic **`sessions.detected`** MUST be **provisioned explicitly**
(partitions, replication factor, documented in quickstart/scripts). The documented reference path MUST
**not** depend on **broker auto-create** as the primary mechanism; production/staging SHOULD mirror
this with **IaC or approved admin tooling**.  
**Scale/Scope**: One probe instance; session detection + dual output paths; no session persistence or
backend API in this feature slice

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: `Session` remains shared-domain entity inheriting `Entity` (no
  surrogate id from probe).
- **SeedWork Immutability**: Only constitution-allowed edits under `SeedWork/`.
- **Boundary Contracts**: Console JSONL (`console-record-schema.md`); Kafka Avro value
  (`session-detected-value.avsc`, `session-detected-avro.md`); capture/output ports
  (`probe-capture-contract.md`, `probe-output-contract.md`). Topic and subject names are stable
  contracts per Article 6–7.
- **Security Controls**: **Article 8** — TLS/mTLS for service-to-service; probe-to-Kafka uses
  **encrypted** transport; **mTLS** in integration/staging/production (ADR 0008). Dev-only relaxation
  must be **documented** (spec FR-016).
- **Containerized Deployables**: Probe Dockerfile maintained; **new**: compose (or docs) for Kafka +
  Registry stack as reference infra for validation (may be separate compose file from
  `docker-compose.probe.yml`).
- **Incremental Compatibility**: Maintainer confirmation if changing published contracts affects
  consumers.
- **Verification Path**: Existing unit/integration tests + **SC-005** path (consume configured
  destination, verify contract).

**Gate status (pre-design)**: PASS — scope extends emission destinations; no constitution violation
if mTLS and contracts are honored.

**Gate status (post-design update in this plan)**: PASS — security updated from “console only” to
**stream transport + authentication posture** aligned with ADR 0008.

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
    ├── Infrastructure/   # TsharkTrafficProvider, ConsolePublisher, future Kafka + Avro publisher
    ├── Host/
    └── NetworkMonitoring.Probe.csproj

tests/
├── NetworkMonitoring.Probe.UnitTests/
└── NetworkMonitoring.Probe.IntegrationTests/
```

**Structure decision**: Add Kafka producer adapter under `Infrastructure/Publishing/` (or
`Infrastructure/Messaging/`), configuration under `Probe` options extension (bootstrap, registry URL,
topic, SSL/mTLS material paths or env), host DI registration conditional on feature flags.

**Dependency inversion**:

- `IMessagePublisher` gains or shares responsibility for `PublishSessionDetected` to Kafka when
  enabled (composite publisher or separate registration strategy — detail in `/speckit.tasks`).
- Use case remains unaware of Avro/Registry; adapter maps `Session` → Avro record per `.avsc`.

## Phase 0 / Research

Consolidated in `research.md` (updated 2026-04-20): operational choices for **Kafka image/stack**
(Apache vs Confluent Platform community images), **listener** layout (internal vs external clients),
**dev PLAINTEXT** vs **TLS** bridge, and **CI** strategy for compose-based tests. All prior Phase 0
decisions for capture/console remain valid.

## Phase 1 / Design artifacts

- **`data-model.md`**: Extended with **event stream** emission notes (logical parity console ↔ Kafka
  value).
- **`contracts/`**: Existing Avro + console contracts are authoritative; no breaking rename without
  explicit version bump.
- **`quickstart.md`**: **Kafka + Registry** bring-up, **explicit** `sessions.detected` provisioning
  (script or documented admin commands), Registry subject setup, and probe configuration for stream
  publication.

## Agent context

Run after plan update: `.specify/scripts/bash/update-agent-context.sh cursor-agent` (adds Kafka,
Registry, Avro, mTLS to agent technology list when executed).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

---

## Next step (Spec Kit)

Follow **`specs/001-session-detection/tasks.md`** for implementation phases: infra (KRaft 3 brokers +
Registry), **explicit topic** `sessions.detected` + Registry subject, mTLS/cert story for non-dev,
probe Kafka publisher + config, and E2E verification for SC-005.
