---
description: "Task list for 003-device-discovery"
---

# Tasks: Device Discovery Separation

**Input**: Design documents from `/specs/003-device-discovery/`  
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Include automated unit/integration tests for discovery validation, schema stability, and consolidation behavior.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
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
- [X] T015 [US1] Align `DeviceDetected` serialization fields with 002 schema contract in `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs`
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

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all stories.
- **User Story Phases (3-5)**: Depend on Foundational completion.
  - US1 delivers MVP.
  - US2 depends on US1 discovery flow baseline.
  - US3 depends on US1/US2 artifact stabilization.
- **Polish (Phase 6)**: Depends on all selected story phases being complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other stories.
- **US2 (P2)**: Starts after US1 baseline emission path is in place.
- **US3 (P3)**: Starts after US1/US2 produce stable references and contracts.

### Within Each User Story

- Tests are added before or alongside implementation and must validate acceptance behavior.
- Domain behavior updates before serialization/contract checks.
- Story checkpoints must pass before moving to next priority.

### Parallel Opportunities

- T002 and T003 can run in parallel.
- T005, T006, T007, T008 can run in parallel after T004.
- US1 tests T009-T011 can run in parallel.
- US2 tests T017-T019 can run in parallel.
- Polish tasks T028 and T030 can run in parallel.

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
5. Final polish and validation.

### Parallel Team Strategy

With multiple contributors:
1. Team completes Setup + Foundational.
2. Contributor A leads US1 flow and contract serialization.
3. Contributor B prepares US2 consolidation tests/behavior.
4. Contributor C handles US3 documentation compatibility hardening.

