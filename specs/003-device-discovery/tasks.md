---
description: "Task list for 003-device-discovery"
---

# Tasks: Device Discovery Separation

**Input**: Design documents from `/specs/003-device-discovery/`  
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Include automated unit/integration tests for discovery validation, schema stability,
consolidation behavior, Kafka Avro mapping, normalized-MAC keys, and opt-in device stream
publication.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare feature artifacts and baseline wiring for discovery-focused implementation.

- [X] T001 Create discovery task scaffolding references in `specs/003-device-discovery/tasks.md` and align local working notes in `specs/003-device-discovery/quickstart.md`
- [X] T002 [P] Add discovery-specific app configuration placeholders in `src/NetworkMonitoring.Probe/appsettings.json` and `src/NetworkMonitoring.Probe/appsettings.Development.json`
- [X] T003 [P] Keep `specs/003-device-discovery/spec.md` scoped to device discovery only (no unrelated functional areas)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared discovery foundations required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 Add SeedWork guardrail check for 002 execution in `specs/003-device-discovery/plan.md` (no edits under `src/NetworkMonitoring.Domain/SeedWork` except constitution-approved exceptions)
- [X] T005 [P] Add explicit maintainer confirmation record for incremental compatibility in `specs/003-device-discovery/plan.md` (Article 23 trace)
- [X] T006 [P] Introduce discovery validation model contract in `src/NetworkMonitoring.Probe/Application/Models/DiscoveryValidationResult.cs` and use-case mapping helpers
- [X] T007 Define discovery output schema serializer boundary alignment in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs` using `specs/003-device-discovery/contracts/device-detected-schema.md`
- [X] T008 Add foundational unit test fixture helpers for discovery observations in `tests/NetworkMonitoring.Probe.UnitTests/Application/UseCases/ProcessObservationsUseCaseTests.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Detect Devices from Observations (Priority: P1) 🎯 MVP

**Goal**: Emit validated `DeviceDetected` records from probe observations in a dedicated discovery flow.

**Independent Test**: Run discovery processing with valid/invalid evidence and verify only valid device records are emitted while processing continues.

### Tests for User Story 1

- [X] T009 [P] [US1] Add unit tests for valid discovery emission in `tests/NetworkMonitoring.Probe.UnitTests/Application/UseCases/ProcessObservationsUseCaseTests.cs`
- [X] T010 [P] [US1] Add unit tests for invalid discovery rejection with continuation in `tests/NetworkMonitoring.Probe.UnitTests/Application/UseCases/ProcessObservationsUseCaseTests.cs`
- [X] T011 [P] [US1] Add schema-level serialization tests for `DeviceDetected` payload fields in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/ConsoleRecordSchemaTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Extract discovery-only processing path from mixed flow in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`
- [X] T013 [US1] Implement explicit validation-result aggregation for discovery inputs in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`
- [X] T014 [US1] Ensure invalid discovery inputs are logged and skipped without exceptions-as-flow in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`
- [X] T015 [US1] Align `DeviceDetected` serialization fields with the 003 device schema contract in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs`
- [X] T016 [US1] Update quickstart verification wording for discovery-only MVP path in `specs/003-device-discovery/quickstart.md`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - Consolidate Repeated Device Detections (Priority: P2)

**Goal**: Apply deterministic consolidation semantics for repeated detections (`first seen`/`last seen` and enrichment behavior).

**Independent Test**: Feed repeated detections for the same device identity and verify stable lifecycle updates.

### Tests for User Story 2

- [X] T017 [P] [US2] Add unit tests for lifecycle timestamp consolidation in `tests/NetworkMonitoring.Probe.UnitTests/Domain/Entities/DeviceTests.cs`
- [X] T018 [P] [US2] Add unit tests for observed IP enrichment behavior in `tests/NetworkMonitoring.Probe.UnitTests/Domain/Entities/DeviceTests.cs`
- [X] T019 [P] [US2] Add integration fixture scenario for repeated device detections in `tests/NetworkMonitoring.Probe.IntegrationTests/ProbeCaptureToConsoleTests.cs`

### Implementation for User Story 2

- [X] T020 [US2] Add deterministic lifecycle update behavior to `Device` entity in `src/NetworkMonitoring.Domain/Shared/Entities/Device.cs`
- [X] T021 [US2] Apply consolidation semantics in discovery use case orchestration in `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`
- [X] T022 [US2] Ensure consolidated output remains schema-stable in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs`
- [X] T023 [US2] Document consolidation rules in `specs/003-device-discovery/data-model.md`

**Checkpoint**: User Stories 1 and 2 are independently testable with deterministic discovery behavior.

---

## Phase 5: User Story 3 - Self-contained device documentation (Priority: P3)

**Goal**: Keep device discovery requirements and supporting docs free of unrelated platform features.

**Independent Test**: Review `specs/003-device-discovery/**/*.md` and confirm nothing prescribes behavior that belongs in another feature’s specification.

### Tests for User Story 3

- [X] T024 [P] [US3] Pass consistency review on `specs/003-device-discovery/spec.md` for device-only scope

### Implementation for User Story 3

- [X] T025 [US3] Keep `specs/003-device-discovery/contracts/device-discovery-contract.md` limited to device publication semantics
- [X] T026 [US3] Align `specs/003-device-discovery/quickstart.md` and `specs/003-device-discovery/research.md` with device-only narrative
- [X] T027 [US3] Align `specs/003-device-discovery/data-model.md` and `specs/003-device-discovery/plan.md` with the same boundary

**Checkpoint**: Device discovery documentation stands on its own without importing external feature requirements.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality checks across discovery implementation and docs.

- [X] T028 [P] Run full test suite and record discovery-focused results in `specs/003-device-discovery/research.md`
- [X] T029 Validate quickstart end-to-end discovery flow and capture outcome notes in `specs/003-device-discovery/quickstart.md`
- [X] T030 [P] Run cross-artifact consistency pass (`spec.md`, `plan.md`, `tasks.md`, `contracts/`) and update `specs/003-device-discovery/checklists/requirements.md`

---

## Phase 7: User Story 4 - Publish Device Detections to Event Stream (Priority: P2)

**Goal**: Publish validated `DeviceDetected` outcomes from the probe to Kafka topic
`devices.detected`, using Avro + Schema Registry and normalized MAC as the Kafka key.

**Independent Test**: With `RUN_KAFKA_INTEGRATION=1` and the reference stack running, publish and
consume one `DeviceDetected` event from `devices.detected`; verify Avro fields match
`device-detected-value.avsc` and the Kafka key equals the normalized MAC.

### Tests for User Story 4

- [X] T031 [P] [US4] Add Avro mapping tests for `DeviceDetected` in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/DeviceDetectedAvroMapperTests.cs`
- [X] T032 [P] [US4] Add normalized-MAC Kafka key tests in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/DeviceKafkaPartitionKeyTests.cs`
- [X] T033 [P] [US4] Add publisher regression coverage for disabled Kafka device publication in `tests/NetworkMonitoring.Probe.UnitTests/Infrastructure/Publishing/KafkaProbeEventPublisherTests.cs`
- [X] T034 [US4] Add opt-in produce/consume integration test for `devices.detected` in `tests/NetworkMonitoring.Probe.IntegrationTests/KafkaDeviceEventPublishIntegrationTests.cs`

### Implementation for User Story 4

- [X] T035 [P] [US4] Add embedded Avro schema `src/NetworkMonitoring.Probe/Infrastructure/Publishing/Schemas/device-detected-value.avsc` and include it in `src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- [X] T036 [P] [US4] Add `KafkaDeviceTopic` defaulting to `devices.detected` in `src/NetworkMonitoring.Probe/Application/Configuration/ProbeOptions.cs`, `src/NetworkMonitoring.Probe/appsettings.json`, and `src/NetworkMonitoring.Probe/appsettings.Development.json`
- [X] T037 [US4] Implement `DeviceDetectedAvroMapper` in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/DeviceDetectedAvroMapper.cs`
- [X] T038 [P] [US4] Implement `DeviceKafkaPartitionKey` in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/DeviceKafkaPartitionKey.cs`
- [X] T039 [US4] Implement Kafka publication for `PublishDeviceDetected` in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/KafkaProbeEventPublisher.cs`
- [X] T040 [US4] Ensure Kafka device publication remains behind `IMessagePublisher` composition in `src/NetworkMonitoring.Probe/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T041 [US4] Update topic bootstrap to create `devices.detected` in `scripts/bootstrap/kafka-topics-init.sh`

**Checkpoint**: User Story 4 is independently testable with the reference Kafka stack and does not
change console discovery behavior or `sessions.detected` publication.

---

## Phase 8: Polish & Cross-Cutting Concerns (Device Stream)

**Purpose**: Final quality checks and documentation consistency for device event-stream publication.

- [X] T042 [P] Update stale session-scope note about `PublishDeviceDetected` no-op in `specs/001-session-detection/contracts/probe-output-contract.md`
- [X] T043 [P] Update `README.md` quickstart/detail text so device stream validation points to `specs/003-device-discovery/quickstart.md`
- [X] T044 Run `dotnet test src/NetworkMonitoring.Probe.sln` and record results in `specs/003-device-discovery/research.md`
- [X] T045 Run or document the gated `RUN_KAFKA_INTEGRATION=1` device publish validation and record SC-005/SC-006 outcome in `specs/003-device-discovery/research.md`
- [X] T046 Run cross-artifact consistency pass for `specs/003-device-discovery/spec.md`, `plan.md`, `tasks.md`, `data-model.md`, `contracts/`, and `quickstart.md`, then update `specs/003-device-discovery/checklists/requirements.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all stories.
- **User Story Phases (3-5)**: Depend on Foundational completion.
  - US1 delivers MVP.
  - US2 depends on US1 discovery flow baseline.
  - US3 depends on US1/US2 artifact stabilization.
- **Historical polish (Phase 6)**: Completed for the console/consolidation baseline.
- **US4 (Phase 7)**: Depends on delivered US1/US2 discovery output and the existing Kafka stack from
  `001-session-detection`.
- **Device-stream polish (Phase 8)**: Depends on US4 implementation and validation.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other stories.
- **US2 (P2)**: Starts after US1 baseline emission path is in place.
- **US3 (P3)**: Starts after US1/US2 produce stable references and contracts.
- **US4 (P2)**: Starts after US1/US2 baseline `DeviceDetected` emission and consolidation are
  delivered; independent of downstream consumers.

### Within Each User Story

- Tests are added before or alongside implementation and must validate acceptance behavior.
- Domain behavior updates before serialization/contract checks.
- Story checkpoints must pass before moving to next priority.

### Parallel Opportunities

- T002 and T003 can run in parallel.
- T005, T006, T007, T008 can run in parallel after T004.
- US1 tests T009-T011 can run in parallel.
- US2 tests T017-T019 can run in parallel.
- US4 tests T031-T033 can run in parallel.
- US4 implementation tasks T035, T036, and T038 can run in parallel after T031-T032 are defined.
- Device-stream polish tasks T042 and T043 can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Run US1 tests in parallel:
Task: "T009 [US1] Valid discovery emission tests"
Task: "T010 [US1] Invalid discovery rejection tests"
Task: "T011 [US1] DeviceDetected schema serialization tests"

# Then implement core flow:
Task: "T012 [US1] Extract discovery-only processing path"
Task: "T013 [US1] Implement explicit validation-result aggregation"
```

---

## Parallel Example: User Story 4

```bash
# Run US4 tests in parallel:
Task: "T031 [US4] DeviceDetected Avro mapper tests"
Task: "T032 [US4] Device Kafka key tests"
Task: "T033 [US4] disabled Kafka device publication test"

# Then implement independent files:
Task: "T035 [US4] embedded device Avro schema"
Task: "T036 [US4] KafkaDeviceTopic configuration"
Task: "T038 [US4] DeviceKafkaPartitionKey"
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Setup and Foundational phases.
2. Deliver US1 discovery emission flow.
3. Validate independent US1 test criteria.
4. Demo discovery-only output behavior.

### Incremental Delivery

1. Baseline foundations.
2. Discovery emission (US1).
3. Consolidation semantics (US2).
4. Compatibility hardening and self-contained ownership (US3).
5. Device event-stream publication (US4).
6. Final device-stream polish and validation.

### Parallel Team Strategy

With multiple contributors:
1. Team completes Setup + Foundational.
2. Contributor A leads US1 flow and contract serialization.
3. Contributor B prepares US2 consolidation tests/behavior.
4. Contributor C handles US3 documentation compatibility hardening.
5. Contributor D handles US4 Kafka mapper/key/publisher and integration validation.

---

## Notes

- Existing completed tasks T001-T030 are preserved as the delivered console/consolidation baseline.
- New tasks T031-T046 cover only probe-side Kafka publication of `DeviceDetected` events to
  `devices.detected`.
- No downstream consumer implementation work is included in this task list.
- **Task count**: T001-T046 complete (**46**). **Total defined: 46.**

