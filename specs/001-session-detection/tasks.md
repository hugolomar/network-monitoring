---
description: "Task list for 001-session-detection (US1–US2 delivered; US3 Elasticsearch query projection)"
---

# Tasks: Probe Session Detection Visibility

**Input**: Design documents from `/home/hugo/network-monitoring/specs/001-session-detection/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**ADRs**: `docs/adr/0006-avro-schema-registry-for-session-kafka-payloads.md`, `docs/adr/0007-kafka-kraft-without-zookeeper.md`, `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md`, `docs/adr/0009-elasticsearch-for-session-detection-query.md`

**Tests**: Unit + integration per plan; add broker-side integration when compose stack exists (SC-005).

**Organization**: Phases 1–4 = completed MVP (**US1**). Phases 5–7 = **User Story 2** (Kafka + Schema Registry, KRaft, TLS/mTLS per ADRs). Phase 8 = **User Story 3** (Elasticsearch read projection + Kafka Connect ingest per **ADR 0009**). **Do not modify `specs/002-device-discovery/` from this list.**

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label for story-phase tasks
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure) — COMPLETE

**Purpose**: Create the probe solution and baseline project structure.

- [X] T001 Create solution file `src/NetworkMonitoring.Probe.sln` and add project folders under `src/`
- [X] T002 Create project file `src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj` and internal folders for Application/Infrastructure/Host layering
- [X] T003 [P] Create test project files `tests/NetworkMonitoring.Probe.UnitTests/NetworkMonitoring.Probe.UnitTests.csproj` and `tests/NetworkMonitoring.Probe.IntegrationTests/NetworkMonitoring.Probe.IntegrationTests.csproj`
- [X] T004 [P] Add shared references and package references in all new `.csproj` files (xUnit, test SDK, hosting/configuration packages, and project references)
- [X] T005 Create baseline probe configuration files `src/NetworkMonitoring.Probe/appsettings.json` and `src/NetworkMonitoring.Probe/appsettings.Development.json`

---

## Phase 2: Foundational (Blocking Prerequisites) — COMPLETE

**Purpose**: Core domain/application abstractions required by all stories.

- [X] T006 Add SeedWork guardrail note in `specs/001-session-detection/plan.md` execution checklist and verify no changes outside `src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj` and `src/NetworkMonitoring.Domain/SeedWork/GlobalUsings.cs`
- [X] T007 [P] Implement ValueObjects (`IpAddress`, `MacAddress`, `Port`) in `src/NetworkMonitoring.Domain/Shared/ValueObjects/` inheriting from `src/NetworkMonitoring.Domain/SeedWork/ValueObject.cs`
- [X] T008 [P] Implement ValueObjects (`ProtocolType`, `DiscoverySource`) in `src/NetworkMonitoring.Domain/Shared/ValueObjects/` inheriting from `src/NetworkMonitoring.Domain/SeedWork/ValueObject.cs`
- [X] T009 Implement shared-domain `Session` entity in `src/NetworkMonitoring.Domain/Shared/Entities/` inheriting from `src/NetworkMonitoring.Domain/SeedWork/Entity.cs`
- [X] T010 [P] Implement normalized capture model (`TrafficObservation`) and validation result model (`ObservationValidationResult`) in `src/NetworkMonitoring.Probe/Application/Models/`
- [X] T011 [P] Define application ports (`ITrafficProvider`, `IMessagePublisher`) in `src/NetworkMonitoring.Probe/Application/Ports/`
- [X] T012 Implement use-case orchestration (`ProcessObservationsUseCase`) in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs` consuming `ITrafficProvider` and `IMessagePublisher`
- [X] T013 [P] Add host wiring abstractions in `src/NetworkMonitoring.Probe/Host/Configuration/ProbeOptions.cs` and `src/NetworkMonitoring.Probe/Host/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation ready.

---

## Phase 3: User Story 1 — Observe captured entities live (Priority: P1) — COMPLETE

**Goal**: Capture traffic and print `SessionDetected` records to console.

**Independent Test**: Run probe with test traffic; at least one session record on console.

### Tests for User Story 1

- [X] T014 [P] [US1] Add ValueObject validation tests in `tests/NetworkMonitoring.Probe.UnitTests/Domain/ValueObjects/IpAddressTests.cs`, `MacAddressTests.cs`, and `PortTests.cs`
- [X] T015 [P] [US1] Add session entity construction tests in `tests/NetworkMonitoring.Probe.UnitTests/Domain/Entities/SessionTests.cs`
- [X] T016 [P] [US1] Add capture mapping tests in `tests/NetworkMonitoring.Probe.UnitTests/Application/Mapping/TsharkObservationMapperTests.cs`

### Implementation for User Story 1

- [X] T017 [US1] Implement tshark line mapper (`TsharkObservationMapper`) in `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkObservationMapper.cs`
- [X] T018 [US1] Implement provider adapter (`TsharkTrafficProvider`) in `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkTrafficProvider.cs` for `ITrafficProvider`
- [X] T019 [US1] Implement console publisher (`ConsolePublisher`) in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsolePublisher.cs` for `IMessagePublisher`
- [X] T020 [US1] Implement JSON envelope serializer (`ConsoleRecordSerializer`) in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs` matching `specs/001-session-detection/contracts/console-record-schema.md`
- [X] T021 [US1] Wire provider/publisher/use case in `src/NetworkMonitoring.Probe/Program.cs`
- [X] T022 [US1] Add entrypoint run service `src/NetworkMonitoring.Probe/Host/Services/ProbeWorker.cs` to execute `ProcessObservationsUseCase`
- [X] T023 [US1] Add integration scenario using fixture stream in `tests/NetworkMonitoring.Probe.IntegrationTests/ProbeCaptureToConsoleTests.cs`
- [X] T024 [US1] Add malformed-observation handling in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs` so processing continues after invalid records
- [X] T025 [US1] Add console schema consistency test in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/ConsoleRecordSchemaTests.cs`

---

## Phase 4: Polish (US1 scope) — COMPLETE

**Purpose**: Hardening and docs for console MVP.

- [X] T026 [P] Update quickstart commands and expected results in `specs/001-session-detection/quickstart.md`
- [X] T027 [P] Add architecture notes for ports/adapters in `specs/001-session-detection/contracts/probe-capture-contract.md` and `specs/001-session-detection/contracts/probe-output-contract.md`
- [X] T028 Run full test suite from `tests/` and record results in `specs/001-session-detection/research.md`
- [X] T029 Validate MVP execution manually and capture checklist completion in `specs/001-session-detection/checklists/requirements.md`
- [X] T030 [P] Add probe containerization artifacts in `src/NetworkMonitoring.Probe/Dockerfile`, `.dockerignore`, and `docker-compose.probe.yml`
- [X] T031 [P] Add container run/build validation steps in `specs/001-session-detection/quickstart.md`
- [X] T032 [P] Refactor ingestion flow to explicit validation-pattern handling in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`
- [X] T033 [P] Add continuation-after-invalid-observation test in `tests/NetworkMonitoring.Probe.UnitTests/Application/UseCases/ProcessObservationsUseCaseTests.cs`

---

## Requirements traceability (Phases 1–4)

| Spec ID | How US1 delivery maps (tasks / code) |
|---------|--------------------------------------|
| **FR-011** | **T012**, **T032**, **T033** — configurable sliding-window suppression of repeated session emissions in `ProcessObservationsUseCase`; `ProbeOptions.SessionDeduplicationWindowMinutes` (default **10** per spec) in `src/NetworkMonitoring.Probe/Application/Configuration/ProbeOptions.cs`, bound from config in **T005** / **T013**. |
| **FR-012** | **T005**, **T013** — single configuration surface: capture interface, tshark path, optional filter, deduplication window via `ProbeOptions` + `appsettings` + `ServiceCollectionExtensions`. |

---

## Phase 5: Platform infrastructure (Kafka KRaft + Schema Registry)

**Purpose**: Reproducible stack for US2 and SC-005. Align with **ADR 0007** (KRaft, ~3 brokers, no ZooKeeper) and **ADR 0008** (TLS/mTLS target; document dev relaxation).

**Topic provisioning (professional standard)**: The reference compose/integration path MUST **explicitly** create topic **`sessions.detected`** (chosen partition count and replication factor documented). **Do not** treat **broker auto-create** as the supported way to obtain that topic in this repo’s documented flow—auto-create may hide misconfiguration and yields **default** (often wrong) partitions/RF. Production/staging SHOULD use the same discipline via **IaC or approved admin tooling** (Terraform, operator, pipeline), not reliance on the first producer.

**⚠️** No edits under `specs/002-device-discovery/`.

- [X] T034 [P] Add reference compose (e.g. `docker-compose.reference-stack.yml` at repository root or path agreed in plan) running **Apache Kafka in KRaft mode** with **three brokers** and **Confluent-compatible Schema Registry**, suitable for local/integration validation per `docs/adr/0007-kafka-kraft-without-zookeeper.md`
- [X] T035 [P] Document bootstrap servers, Registry URL, listener ports, and **security posture** (production mTLS vs documented non-production relaxation) in `specs/001-session-detection/quickstart.md` per `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md`
- [X] T036 Add **`scripts/kafka-topics-init.sh`** (or equivalent) that **explicitly** creates topic **`sessions.detected`** with **documented** partition count and replication factor for the reference stack; script MUST be invocable after brokers are healthy (compose `depends_on` + healthcheck, documented manual step, or `make` target). Include **idempotent** behavior where practical (`--if-not-exists` or equivalent).
- [X] T037 Document in `specs/001-session-detection/quickstart.md`: (1) order of operations—compose up → run topic init → verify topic; (2) Registry subject **`sessions.detected-value`** and first schema registration path per `specs/001-session-detection/contracts/session-detected-avro.md`; (3) **production note**: same explicit provisioning via IaC/pipeline, not auto-create as policy.

**Checkpoint**: `docker compose` (or equivalent) brings up Kafka + Registry; **`sessions.detected` exists by explicit init**, not as an undocumented side effect of auto-create; operators follow quickstart end-to-end.

---

## Phase 6: User Story 2 — Session event stream (Kafka + Avro) (Priority: P2)

**Goal**: Publish validated session detections to Kafka as **Avro** values per **`session-detected-value.avsc`**, subject **`sessions.detected-value`**, default topic **`sessions.detected`**, with **message key** matching session deduplication identity (spec FR-014). Encoding per **ADR 0006**; transport per **ADR 0008**.

**Independent Test**: With stack up and publication enabled, consume `sessions.detected` and verify Avro payload matches contract (SC-005).

### Tests for User Story 2

- [X] T038 [P] [US2] Add unit tests for session→Avro field mapping and key serialization helpers in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/` (new files as needed)
- [X] T039 [US2] Add integration test that produces to a test broker or Testcontainers-based Kafka (if adopted) in `tests/NetworkMonitoring.Probe.IntegrationTests/` validating end-to-end publish + consume of one `SessionDetected` record (skip or `[Fact(Skip=...)]` until compose wiring stable if necessary)

### Implementation for User Story 2

- [X] T040 [P] [US2] Add NuGet packages for `Confluent.Kafka`, `Confluent.SchemaRegistry`, and Avro serde (e.g. `Confluent.SchemaRegistry.Serdes.Avro` or project-standard equivalents) to `src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- [X] T041 [US2] Extend `src/NetworkMonitoring.Probe/Application/Configuration/ProbeOptions.cs` (and `appsettings.json` / env templates) with: `EnableKafka`, `KafkaBootstrapServers`, `SchemaRegistryUrl`, `KafkaSessionTopic` (default `sessions.detected`), SSL/mTLS-related settings (cert paths, passwords) aligned with ADR 0008
- [X] T042 [US2] Implement Avro serialization for `SessionDetected` consistent with `specs/001-session-detection/contracts/session-detected-value.avsc` in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/` (e.g. `SessionDetectedAvroSerializer.cs` or equivalent)
- [X] T043 [US2] Implement `KafkaSessionPublisher` (or named adapter) in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/` that publishes Avro values and sets **partition key** from the same session identity fields as deduplication (see `ProcessObservationsUseCase` / spec FR-014)
- [X] T044 [US2] Register publisher(s) in `src/NetworkMonitoring.Probe/Host/DependencyInjection/ServiceCollectionExtensions.cs`: support **config-driven** output—e.g. console only, Kafka only, or both (composite) via `Probe` flags—so **use-case code** does not branch on infrastructure (hexagonal boundary); default for **documented production** SHOULD be Kafka-capable with console **off** unless operators need local visibility
- [X] T045 [US2] Update `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs` **only if** key derivation or publish hooks must remain DRY; avoid leaking Kafka types into Application layer
- [X] T046 [P] [US2] Update `specs/001-session-detection/contracts/probe-output-contract.md` with Kafka adapter file references and `PublishSessionDetected` behavior

**Checkpoint**: Probe can emit to Kafka with console unchanged; invalid observations still skip Kafka publish.

---

## Phase 7: Polish & verification (US2 / cross-cutting)

**Purpose**: Docs, constitution check, SC-005 evidence.

- [X] T047 [P] Update `specs/001-session-detection/quickstart.md` with end-to-end **Kafka publish** validation steps and troubleshooting
- [X] T048 [P] Record Kafka/Registry image versions and compatibility notes in `specs/001-session-detection/research.md`
- [X] T049 Run `dotnet test /home/hugo/network-monitoring/src/NetworkMonitoring.Probe.sln` and note results in `research.md`
- [X] T050 Manual SC-005 checklist: consume `sessions.detected`, verify 100% sampled messages match `session-detected-value.avsc` semantics; document outcome in `specs/001-session-detection/research.md` or checklist artifact

---

## Phase 8: User Story 3 — Queryable session history (Elasticsearch + Kafka Connect) (Priority: P3)

**Purpose**: Satisfy **FR-017–FR-021** and **SC-006** with **Elasticsearch** as a **query projection** of `SessionDetected` events; **Kafka** remains the log. Reference ingest: **Kafka Connect** Elasticsearch **Sink** from **`sessions.detected`** per **`docs/adr/0009-elasticsearch-for-session-detection-query.md`** and `plan.md` (US3).

**Independent Test**: With Kafka topic populated and connector running, **query** Elasticsearch using documented filters (time range, normalized addresses, ports when applicable, protocol); sampled hits match **`contracts/session-detected-value.avsc`** semantics (**SC-006**). Results use **bounded** page size / pagination (**FR-019**).

**Prerequisites**: `docker-compose.reference-stack.yml`, `scripts/kafka-topics-init.sh`, and probe Kafka path (Phases 5–7) operational.

**⚠️** No edits under `specs/002-device-discovery/`.

- [ ] T051 [US3] Extend **`docker-compose.reference-stack.yml`** at repository root with **Elasticsearch** and **Kafka Connect** worker service(s) on the existing **`kafka-net`** network so one `up` can run the full reference stack; document in `specs/001-session-detection/quickstart.md` the **Kafka-only** variant via explicit service list (e.g. brokers + Schema Registry without ES/Connect)
- [ ] T052 [US3] Add `scripts/verify-elasticsearch-stack.sh` that checks Elasticsearch and Kafka Connect HTTP health (mirror conventions in `scripts/verify-kafka-stack.sh`) and prints actionable failures
- [ ] T053 [P] [US3] Add `specs/001-session-detection/contracts/elasticsearch-session-detected-mapping.md` documenting Avro → Elasticsearch field mapping from `specs/001-session-detection/contracts/session-detected-value.avsc` (normalization **FR-006**, semantic parity **FR-018**)
- [ ] T054 [P] [US3] Add Elasticsearch index template (or data stream) JSON under `scripts/elasticsearch/index-template-sessions-detected.json` plus `scripts/elasticsearch/apply-index-template.sh` (or equivalent) to apply mappings from `elasticsearch-session-detected-mapping.md` in dev
- [ ] T055 [US3] Add connector config `scripts/connectors/elasticsearch-sink-sessions-detected.json` for the Elasticsearch Sink: consume **`sessions.detected`**, target index/data stream name, key/id strategy documented to control duplicates (align with spec edge cases on documented dedup semantics)
- [ ] T056 [US3] Add `scripts/register-elasticsearch-sink-connector.sh` that registers the connector via Connect REST API with idempotent behavior where the API allows (create-or-update pattern documented on failure)
- [ ] T057 [US3] Extend `specs/001-session-detection/quickstart.md` with end-to-end **US3** steps: unified compose **full stack** (or Kafka-only subset) → topic → ES/Connect healthy → index template → register connector → probe publish → **Elasticsearch `_search`** examples using **`size`**, sort, and `search_after` or explicit limits (**FR-019**)
- [ ] T058 [P] [US3] Document **emission-to-query** latency and **eventual consistency** expectations for operators (**FR-020**) in `specs/001-session-detection/quickstart.md` and add a short measured note in `specs/001-session-detection/research.md`
- [ ] T059 [P] [US3] Document **TLS** and **authentication** for Elasticsearch and Connect in non-dev vs documented dev relaxation (`specs/001-session-detection/quickstart.md`) consistent with **ADR 0009** and constitution Article 8 (**FR-021** at operational level)
- [ ] T060 [US3] Add `scripts/verify-sc006-elasticsearch-sampling.sh` (or extend `scripts/verify-elasticsearch-stack.sh`) that runs a **bounded** query, samples hits, and fails if required fields/semantics diverge from `session-detected-value.avsc` (gate with an env flag such as `RUN_ES_INTEGRATION=1` if needed)
- [ ] T061 [P] [US3] Pin **Elasticsearch** and **Kafka Connect** image versions and connector plugin strategy in `specs/001-session-detection/research.md` (follow-up to Decision 19) and cross-reference in `specs/001-session-detection/quickstart.md`

**Checkpoint**: Operators can bring up the **unified** reference compose (Kafka + Registry + ES + Connect), index session detections, and reproduce **SC-006** using the documented query + script path; **Kafka-only** remains documented for lighter runs.

---

## Dependencies & Execution Order

| Phase | Depends on |
|-------|------------|
| 1–4 | (complete) |
| 5 | 1–4 |
| 6 | 5 (brokers + Registry reachable); 1–4 |
| 7 | 6 |
| 8 | 5–7 (topic + Avro stream available); **US3** assumes **US2** path per spec |

**User stories**: US2 builds on US1; US3 builds on US2 (events on stream before query projection); US1 remains independently demoable via console.

---

## Parallel opportunities

- T034 and T035 after plan approval
- T038 and T040 early in Phase 6 (different files)
- T047 and T048 in Phase 7
- **US3**: T053, T054, T058, T059, T061 after T051 (mapping/docs vs infra scripts); T052 can follow T051

---

## Implementation strategy

1. Bring up **`docker-compose.reference-stack.yml`** (Phase 5); confirm topic + subject.
2. Implement **Phase 6** publisher behind `IMessagePublisher`; keep domain untouched.
3. **Phase 7**: automate what you can; document manual SC-005 until CI has broker.
4. **Phase 8**: add ES + Connect stack; mapping + connector; document **FR-019–FR-021** and **SC-006** verification.

---

## Notes

- **SeedWork**: no edits except allowed `NetworkMonitoring.Domain.csproj` / `GlobalUsings.cs`.
- **Contracts**: `sessions.detected` / `sessions.detected-value` are stable per constitution Article 6–7; breaking changes need explicit version strategy. **US3** mapping docs must stay aligned (**FR-018**).
- **Kafka topic**: reference and production paths rely on **explicit** topic creation (Phase 5); align staging/prod with team IaC standards.
- **`/speckit.analyze`** recommended before **`/speckit.implement`** once tasks are checked off.
- Path to this file: `/home/hugo/network-monitoring/specs/001-session-detection/tasks.md`
- **Task count**: T001–T050 **complete** (**50**). **T051–T061** open (**11** new). **Total defined: 61.**
