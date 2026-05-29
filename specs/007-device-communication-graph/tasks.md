# Tasks: Device Communication Graph

**Input**: Design documents from `/specs/007-device-communication-graph/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Integration and contract tests are required by the specification and constitution verification gates.  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare feature scaffolding, graph options, and baseline connector/documentation artifacts.

- [X] T001 Create graph feature scaffolding placeholders in `src/NetworkMonitoring.Backend/Host/Endpoints/.gitkeep`, `src/NetworkMonitoring.Backend/Host/Services/.gitkeep`, `src/NetworkMonitoring.Backend/Application/UseCases/.gitkeep`, `src/NetworkMonitoring.Backend/Application/Ports/.gitkeep`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/.gitkeep`
- [X] T002 Add graph option defaults/caps/retry/retention flags in `src/NetworkMonitoring.Backend/Application/Configuration/BackendOptions.cs` and `src/NetworkMonitoring.Backend/appsettings.json`
- [X] T003 [P] Add connector baseline for `sessions.enriched` projection in `connectors/neo4j-sink-sessions-enriched.json`
- [ ] T004 [P] Align quickstart + contract with defaults/error payload/auth clarifications in `specs/007-device-communication-graph/quickstart.md` and `specs/007-device-communication-graph/contracts/graph-api.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and plumbing required before any user story implementation.  
**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Define shared graph domain models in `src/NetworkMonitoring.Backend/Application/Models/GraphNode.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphEdge.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphQueryResult.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphRetentionSweepOutcome.cs`, and `src/NetworkMonitoring.Backend/Application/Models/GraphProjectionDiagnostics.cs`
- [X] T006 Define graph ports in `src/NetworkMonitoring.Backend/Application/Ports/IGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphProjectionRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphRetentionRepository.cs`, and `src/NetworkMonitoring.Backend/Application/Ports/IGraphTelemetry.cs`
- [X] T007 Implement graph adapter skeletons and shared in-memory store in `src/NetworkMonitoring.Backend/Infrastructure/Graph/InMemoryGraphStore.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [X] T008 Wire graph repositories/telemetry/use-case dependencies in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T009 Implement shared graph error translation payload helpers in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphErrorResponses.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [X] T010 [P] Add graph integration test host fixture in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/GraphTestApplicationFactory.cs`

**Checkpoint**: Foundation complete - user stories can now be implemented independently.

---

## Phase 3: User Story 1 - Build Communication Links from Session Traffic (Priority: P1) 🎯 MVP

**Goal**: Project enriched session observations into idempotent communication relationships with bounded retry behavior.

**Independent Test**: Inject representative enriched observations (internal/internal, internal/external, replay duplicates, transient write failure) and verify identity uniqueness, monotonic updates, and continuation after retry exhaustion.

### Tests for User Story 1

- [X] T011 [P] [US1] Add integration test for edge identity `(source,destination,protocol)` uniqueness in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdentityTests.cs`
- [X] T012 [P] [US1] Add integration test for replay idempotency and first/lastSeen/weight behavior in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdempotencyTests.cs`
- [X] T013 [P] [US1] Add integration test for bounded retry and post-failure continuation in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionRetryPolicyTests.cs`

### Implementation for User Story 1

- [X] T014 [US1] Implement projection orchestration use case with retry semantics in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`
- [X] T015 [US1] Implement destination identity mapping helper for internal/external targets in `src/NetworkMonitoring.Backend/Infrastructure/Graph/GraphProjectionMapper.cs`
- [X] T016 [US1] Implement idempotent projection repository write behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`
- [X] T017 [US1] Implement projection telemetry emission bridge in `src/NetworkMonitoring.Backend/Infrastructure/Graph/NullGraphTelemetry.cs` and `src/NetworkMonitoring.Backend/Application/Models/GraphProjectionDiagnostics.cs`
- [X] T018 [US1] Wire projection execution entry and dependency registration in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T019 [US1] Add API/boundary XML docs and rationale comments for projection flow in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`

**Checkpoint**: User Story 1 independently functional and testable.

---

## Phase 4: User Story 2 - Explore a Device-Centered Subgraph (Priority: P1)

**Goal**: Expose authenticated bounded graph retrieval with depth/limit clamping and truncation signaling.

**Independent Test**: Call `GET /api/graph/devices` with varied depth/limit/auth states and validate contract shape, clamping, truncation, and auth outcomes.

### Tests for User Story 2

- [X] T020 [P] [US2] Add contract test for graph endpoint response shape in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesContractTests.cs`
- [X] T021 [P] [US2] Add integration test for depth/limit defaults, caps, and truncation semantics in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphQueryBoundingTests.cs`
- [X] T022 [P] [US2] Add integration test for role-based read access matrix and unauthenticated rejection in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesAuthorizationTests.cs`

### Implementation for User Story 2

- [X] T023 [US2] Implement bounded graph query use case (defaults, clamps, validation) in `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`
- [X] T024 [US2] Implement graph query repository traversal behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`
- [X] T025 [US2] Implement endpoint DTO contracts in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesRequestDto.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesResponseDto.cs`
- [X] T026 [US2] Implement `GET /api/graph/devices` endpoint mapping and role authorization checks in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [X] T027 [US2] Add endpoint/query documentation and rationale comments in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs` and `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`

**Checkpoint**: User Stories 1 and 2 independently functional.

---

## Phase 5: User Story 3 - Enforce Graph Retention Hygiene (Priority: P2)

**Goal**: Run scheduled-only retention cleanup for stale edges and orphan external hosts while preserving internal devices.

**Independent Test**: Seed stale and boundary-equal graph records, execute retention cycle, and verify stale removal + orphan cleanup + internal-device preservation.

### Tests for User Story 3

- [X] T028 [P] [US3] Add integration test for stale-edge pruning (`lastSeen < cutoff`) in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionEdgePruningTests.cs`
- [X] T029 [P] [US3] Add integration test for orphan external-host cleanup and internal-device preservation in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionNodeCleanupTests.cs`
- [X] T030 [P] [US3] Add integration test ensuring no manual/on-demand retention trigger path exists in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphRetentionManualTriggerAbsenceTests.cs`

### Implementation for User Story 3

- [X] T031 [US3] Implement retention sweep use case in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`
- [X] T032 [US3] Implement retention repository deletion and orphan cleanup logic in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [X] T033 [US3] Implement scheduled retention hosted service (24h cadence) in `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [X] T034 [US3] Wire retention hosted service/options in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T035 [US3] Add retention flow XML docs and rationale comments in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs` and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`

**Checkpoint**: User Stories 1-3 independently functional.

---

## Phase 6: User Story 4 - Graceful Degradation on Graph Store Outage (Priority: P2)

**Goal**: Return explicit `503` for graph endpoint outages while preserving non-graph endpoint availability.

**Independent Test**: Simulate graph dependency failure and verify graph endpoint degradation plus continuity of existing non-graph endpoint families.

### Tests for User Story 4

- [X] T036 [P] [US4] Add integration test for graph outage -> `503` contract payload in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphUnavailableContractTests.cs`
- [X] T037 [P] [US4] Add integration test for non-graph endpoint continuity during graph outage in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphOutageIsolationTests.cs`

### Implementation for User Story 4

- [X] T038 [US4] Implement endpoint unavailable mapping (`code`, `message`, `traceId`, `timestampUtc`) in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [X] T039 [US4] Implement graph exception-to-error-code translation behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [X] T040 [US4] Add guardrails that keep non-graph endpoint wiring unchanged under graph faults in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`

**Checkpoint**: All user stories independently functional and outage isolation validated.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, observability baseline, constitutional compliance, and release readiness.

- [X] T041 [P] Implement structured logs and counters/latency metrics for projection + retention in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`, `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`, and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [X] T042 Add integration test for observability baseline signals in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphObservabilitySignalsTests.cs`
- [X] T043 Run graph-focused verification suite (`dotnet test tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj --filter "FullyQualifiedName~Graph"`) and record SC-001 latency evidence in `specs/007-device-communication-graph/quickstart.md`
- [X] T044 [P] Execute outage-isolation + role-policy + defaults validation walkthrough and record evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T045 Verify constitutional compliance in `specs/007-device-communication-graph/plan.md`, including explicit SeedWork immutability check for `src/NetworkMonitoring.Domain/SeedWork/` (Article 21)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2; validation benefits from US1-projected data.
- **Phase 5 (US3)**: Depends on Phase 2 and graph model produced by US1 behavior.
- **Phase 6 (US4)**: Depends on Phase 4 endpoint path and foundational error mapping.
- **Phase 7 (Polish)**: Depends on all targeted user stories complete.

### User Story Dependency Graph

- **US1 (P1)**: MVP slice; no user-story dependency.
- **US2 (P1)**: Requires foundational graph abstractions.
- **US3 (P2)**: Requires foundational graph abstractions and retention model.
- **US4 (P2)**: Requires graph endpoint from US2 and error mapping foundation.

### Parallel Opportunities

- Setup `[P]`: T003, T004.
- Foundational `[P]`: T010.
- US1 test tasks `[P]`: T011, T012, T013.
- US2 test tasks `[P]`: T020, T021, T022.
- US3 test tasks `[P]`: T028, T029, T030.
- US4 test tasks `[P]`: T036, T037.
- Polish `[P]`: T041, T044.

---

## Parallel Example: User Story 1

```bash
# Run US1 tests in parallel:
Task: "T011 [US1] ...GraphProjectionIdentityTests.cs"
Task: "T012 [US1] ...GraphProjectionIdempotencyTests.cs"
Task: "T013 [US1] ...GraphProjectionRetryPolicyTests.cs"

# Then implement independent artifacts in parallel:
Task: "T015 [US1] ...GraphProjectionMapper.cs"
Task: "T017 [US1] ...NullGraphTelemetry.cs / GraphProjectionDiagnostics.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate identity, idempotency, and retry-continuation before expanding scope.

### Incremental Delivery

1. Add US2 for bounded authenticated graph retrieval.
2. Add US3 for scheduled retention hygiene.
3. Add US4 for outage-isolation hardening.
4. Finish with observability and measurable outcome evidence in Phase 7.

### Suggested MVP Scope

- **MVP**: Phase 1 + Phase 2 + Phase 3 (US1 only).
- Delivers immediate value by enabling communication graph projection with objective verification.

---

## Notes

- All tasks use required checklist format: checkbox + Task ID + optional `[P]` + required `[USx]` for story tasks + exact file paths.
- Story phases maintain independent testability and align with FR-014/015/016/017 plus SC-006 obligations.
# Tasks: Device Communication Graph

**Input**: Design documents from `/specs/007-device-communication-graph/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Integration and contract tests are required by the specification and constitution verification gates.  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare feature scaffolding, graph options, and baseline connector/documentation artifacts.

- [ ] T001 Create graph feature scaffolding placeholders in `src/NetworkMonitoring.Backend/Host/Endpoints/.gitkeep`, `src/NetworkMonitoring.Backend/Host/Services/.gitkeep`, `src/NetworkMonitoring.Backend/Application/UseCases/.gitkeep`, `src/NetworkMonitoring.Backend/Application/Ports/.gitkeep`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/.gitkeep`
- [ ] T002 Add graph option defaults/caps/retry/retention flags in `src/NetworkMonitoring.Backend/Application/Configuration/BackendOptions.cs` and `src/NetworkMonitoring.Backend/appsettings.json`
- [ ] T003 [P] Add connector baseline for `sessions.enriched` projection in `connectors/neo4j-sink-sessions-enriched.json`
- [ ] T004 [P] Align quickstart + contract with defaults/error payload/auth clarifications in `specs/007-device-communication-graph/quickstart.md` and `specs/007-device-communication-graph/contracts/graph-api.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and plumbing required before any user story implementation.  
**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Define shared graph domain models in `src/NetworkMonitoring.Backend/Application/Models/GraphNode.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphEdge.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphQueryResult.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphRetentionSweepOutcome.cs`, and `src/NetworkMonitoring.Backend/Application/Models/GraphProjectionDiagnostics.cs`
- [ ] T006 Define graph ports in `src/NetworkMonitoring.Backend/Application/Ports/IGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphProjectionRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphRetentionRepository.cs`, and `src/NetworkMonitoring.Backend/Application/Ports/IGraphTelemetry.cs`
- [ ] T007 Implement graph adapter skeletons and shared in-memory store in `src/NetworkMonitoring.Backend/Infrastructure/Graph/InMemoryGraphStore.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [ ] T008 Wire graph repositories/telemetry/use-case dependencies in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T009 Implement shared graph error translation payload helpers in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphErrorResponses.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [ ] T010 [P] Add graph integration test host fixture in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/GraphTestApplicationFactory.cs`

**Checkpoint**: Foundation complete - user stories can now be implemented independently.

---

## Phase 3: User Story 1 - Build Communication Links from Session Traffic (Priority: P1) 🎯 MVP

**Goal**: Project enriched session observations into idempotent communication relationships with bounded retry behavior.

**Independent Test**: Inject representative enriched observations (internal/internal, internal/external, replay duplicates, transient write failure) and verify identity uniqueness, monotonic updates, and continuation after retry exhaustion.

### Tests for User Story 1

- [ ] T011 [P] [US1] Add integration test for edge identity `(source,destination,protocol)` uniqueness in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdentityTests.cs`
- [ ] T012 [P] [US1] Add integration test for replay idempotency and first/lastSeen/weight behavior in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdempotencyTests.cs`
- [ ] T013 [P] [US1] Add integration test for bounded retry and post-failure continuation in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionRetryPolicyTests.cs`

### Implementation for User Story 1

- [ ] T014 [US1] Implement projection orchestration use case with retry semantics in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`
- [ ] T015 [US1] Implement destination identity mapping helper for internal/external targets in `src/NetworkMonitoring.Backend/Infrastructure/Graph/GraphProjectionMapper.cs`
- [ ] T016 [US1] Implement idempotent projection repository write behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`
- [ ] T017 [US1] Implement projection telemetry emission bridge in `src/NetworkMonitoring.Backend/Infrastructure/Graph/NullGraphTelemetry.cs` and `src/NetworkMonitoring.Backend/Application/Models/GraphProjectionDiagnostics.cs`
- [ ] T018 [US1] Wire projection execution entry and dependency registration in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T019 [US1] Add API/boundary XML docs and rationale comments for projection flow in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`

**Checkpoint**: User Story 1 independently functional and testable.

---

## Phase 4: User Story 2 - Explore a Device-Centered Subgraph (Priority: P1)

**Goal**: Expose authenticated bounded graph retrieval with depth/limit clamping and truncation signaling.

**Independent Test**: Call `GET /api/graph/devices` with varied depth/limit/auth states and validate contract shape, clamping, truncation, and auth outcomes.

### Tests for User Story 2

- [ ] T020 [P] [US2] Add contract test for graph endpoint response shape in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesContractTests.cs`
- [ ] T021 [P] [US2] Add integration test for depth/limit defaults, caps, and truncation semantics in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphQueryBoundingTests.cs`
- [ ] T022 [P] [US2] Add integration test for auth-required access and unauthenticated rejection in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesAuthorizationTests.cs`

### Implementation for User Story 2

- [ ] T023 [US2] Implement bounded graph query use case (defaults, clamps, validation) in `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`
- [ ] T024 [US2] Implement graph query repository traversal behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`
- [ ] T025 [US2] Implement endpoint DTO contracts in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesRequestDto.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesResponseDto.cs`
- [ ] T026 [US2] Implement `GET /api/graph/devices` endpoint mapping and auth checks in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [ ] T027 [US2] Add endpoint/query documentation and rationale comments in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs` and `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`

**Checkpoint**: User Stories 1 and 2 independently functional.

---

## Phase 5: User Story 3 - Enforce Graph Retention Hygiene (Priority: P2)

**Goal**: Run scheduled-only retention cleanup for stale edges and orphan external hosts while preserving internal devices.

**Independent Test**: Seed stale and boundary-equal graph records, execute retention cycle, and verify stale removal + orphan cleanup + internal-device preservation.

### Tests for User Story 3

- [ ] T028 [P] [US3] Add integration test for stale-edge pruning (`lastSeen < cutoff`) in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionEdgePruningTests.cs`
- [ ] T029 [P] [US3] Add integration test for orphan external-host cleanup and internal-device preservation in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionNodeCleanupTests.cs`
- [ ] T030 [P] [US3] Add integration test ensuring no manual/on-demand retention trigger path exists in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphRetentionManualTriggerAbsenceTests.cs`

### Implementation for User Story 3

- [ ] T031 [US3] Implement retention sweep use case in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`
- [ ] T032 [US3] Implement retention repository deletion and orphan cleanup logic in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [ ] T033 [US3] Implement scheduled retention hosted service (24h cadence) in `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [ ] T034 [US3] Wire retention hosted service/options in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T035 [US3] Add retention flow XML docs and rationale comments in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs` and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`

**Checkpoint**: User Stories 1-3 independently functional.

---

## Phase 6: User Story 4 - Graceful Degradation on Graph Store Outage (Priority: P2)

**Goal**: Return explicit `503` for graph endpoint outages while preserving non-graph endpoint availability.

**Independent Test**: Simulate graph dependency failure and verify graph endpoint degradation plus continuity of existing non-graph endpoint families.

### Tests for User Story 4

- [ ] T036 [P] [US4] Add integration test for graph outage -> `503` contract payload in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphUnavailableContractTests.cs`
- [ ] T037 [P] [US4] Add integration test for non-graph endpoint continuity during graph outage in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphOutageIsolationTests.cs`

### Implementation for User Story 4

- [ ] T038 [US4] Implement endpoint unavailable mapping (`code`, `message`, `traceId`, `timestampUtc`) in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [ ] T039 [US4] Implement graph exception-to-error-code translation behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [ ] T040 [US4] Add guardrails that keep non-graph endpoint wiring unchanged under graph faults in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`

**Checkpoint**: All user stories independently functional and outage isolation validated.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, observability baseline, constitutional compliance, and release readiness.

- [ ] T041 [P] Implement structured logs and counters/latency metrics for projection + retention in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`, `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`, and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [ ] T042 Add integration test for observability baseline signals in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphObservabilitySignalsTests.cs`
- [ ] T043 Run graph-focused verification suite (`dotnet test tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj --filter "FullyQualifiedName~Graph"`) and record SC-001 latency evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T044 [P] Execute outage-isolation + auth-policy + defaults validation walkthrough and record evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T045 Verify constitutional compliance in `specs/007-device-communication-graph/plan.md`, including explicit SeedWork immutability check for `src/NetworkMonitoring.Domain/SeedWork/` (Article 21)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2; validation benefits from US1-projected data.
- **Phase 5 (US3)**: Depends on Phase 2 and graph model produced by US1 behavior.
- **Phase 6 (US4)**: Depends on Phase 4 endpoint path and foundational error mapping.
- **Phase 7 (Polish)**: Depends on all targeted user stories complete.

### User Story Dependency Graph

- **US1 (P1)**: MVP slice; no user-story dependency.
- **US2 (P1)**: Requires foundational graph abstractions.
- **US3 (P2)**: Requires foundational graph abstractions and retention model.
- **US4 (P2)**: Requires graph endpoint from US2 and error mapping foundation.

### Parallel Opportunities

- Setup `[P]`: T003, T004.
- Foundational `[P]`: T010.
- US1 test tasks `[P]`: T011, T012, T013.
- US2 test tasks `[P]`: T020, T021, T022.
- US3 test tasks `[P]`: T028, T029, T030.
- US4 test tasks `[P]`: T036, T037.
- Polish `[P]`: T041, T044.

---

## Parallel Example: User Story 1

```bash
# Run US1 tests in parallel:
Task: "T011 [US1] ...GraphProjectionIdentityTests.cs"
Task: "T012 [US1] ...GraphProjectionIdempotencyTests.cs"
Task: "T013 [US1] ...GraphProjectionRetryPolicyTests.cs"

# Then implement independent artifacts in parallel:
Task: "T015 [US1] ...GraphProjectionMapper.cs"
Task: "T017 [US1] ...NullGraphTelemetry.cs / GraphProjectionDiagnostics.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate identity, idempotency, and retry-continuation before expanding scope.

### Incremental Delivery

1. Add US2 for bounded authenticated graph retrieval.
2. Add US3 for scheduled retention hygiene.
3. Add US4 for outage-isolation hardening.
4. Finish with observability and measurable outcome evidence in Phase 7.

### Suggested MVP Scope

- **MVP**: Phase 1 + Phase 2 + Phase 3 (US1 only).
- Delivers immediate value by enabling communication graph projection with objective verification.

---

## Notes

- All tasks use required checklist format: checkbox + Task ID + optional `[P]` + required `[USx]` for story tasks + exact file paths.
- Story phases maintain independent testability and align with FR-014/015/016/017 plus SC-006 obligations.
# Tasks: Device Communication Graph

**Input**: Design documents from `/specs/007-device-communication-graph/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The feature specification includes objective verification paths and measurable outcomes; integration and contract test tasks are included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare graph feature scaffolding, options, and connector baseline.

- [ ] T001 Create graph feature scaffolding by adding `.gitkeep` placeholders in `src/NetworkMonitoring.Backend/Host/Endpoints/.gitkeep`, `src/NetworkMonitoring.Backend/Host/Services/.gitkeep`, `src/NetworkMonitoring.Backend/Application/UseCases/.gitkeep`, `src/NetworkMonitoring.Backend/Application/Ports/.gitkeep`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/.gitkeep`
- [ ] T002 Add graph options (depth cap, limit defaults, retention window/cadence, retry bounds, observability toggles) in `src/NetworkMonitoring.Backend/Application/Configuration/BackendOptions.cs` and `src/NetworkMonitoring.Backend/appsettings.json`
- [ ] T003 [P] Add connector projection config baseline in `connectors/neo4j-sink-sessions-enriched.json`
- [ ] T004 [P] Align graph contract and quickstart docs with latest clarifications in `specs/007-device-communication-graph/contracts/graph-api.md` and `specs/007-device-communication-graph/quickstart.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core graph abstractions, wiring, and shared failure handling required by all stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Define shared graph models (`GraphNode`, `GraphEdge`, `GraphQueryResult`, `GraphRetentionSweepOutcome`, `GraphProjectionDiagnostics`) in `src/NetworkMonitoring.Backend/Application/Models/GraphNode.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphEdge.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphQueryResult.cs`, `src/NetworkMonitoring.Backend/Application/Models/GraphRetentionSweepOutcome.cs`, and `src/NetworkMonitoring.Backend/Application/Models/GraphProjectionDiagnostics.cs`
- [ ] T006 Define graph ports (query, projection upsert, retention, telemetry) in `src/NetworkMonitoring.Backend/Application/Ports/IGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphProjectionRepository.cs`, `src/NetworkMonitoring.Backend/Application/Ports/IGraphRetentionRepository.cs`, and `src/NetworkMonitoring.Backend/Application/Ports/IGraphTelemetry.cs`
- [ ] T007 Implement infrastructure adapter skeletons for graph query/projection/retention in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [ ] T008 Wire graph services, options, and hosted services in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T009 Implement shared graph error translation primitives (invalid request, unavailable graph store, retry exhausted) in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphErrorResponses.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [ ] T010 [P] Create shared graph integration test fixtures/harness in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/GraphTestApplicationFactory.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Build Communication Links from Session Traffic (Priority: P1) 🎯 MVP

**Goal**: Project enriched session traffic into unique communication relationships with idempotent updates and bounded retry behavior.

**Independent Test**: Feed representative enriched events (internal/internal, internal/external, replay duplicates, transient write failures) and verify uniqueness, monotonic updates, bounded retries, and continuity.

### Tests for User Story 1

- [ ] T011 [P] [US1] Add integration test for projection identity uniqueness `(source,destination,protocol)` in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdentityTests.cs`
- [ ] T012 [P] [US1] Add integration test for replay idempotency and first/lastSeen/weight semantics in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionIdempotencyTests.cs`
- [ ] T013 [P] [US1] Add integration test for bounded retry then continue-processing behavior in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphProjectionRetryPolicyTests.cs`

### Implementation for User Story 1

- [ ] T014 [US1] Implement projection upsert use case in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`
- [ ] T015 [US1] Implement internal/external destination mapping in `src/NetworkMonitoring.Backend/Infrastructure/Graph/GraphProjectionMapper.cs`
- [ ] T016 [US1] Implement idempotent upsert repository logic in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`
- [ ] T017 [US1] Implement bounded retry policy and retry-exhausted diagnostics in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`
- [ ] T018 [US1] Register projection flow and dependencies in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T019 [US1] Add API/boundary docs and complexity rationale comments (Articles 29-31) in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs` and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`

**Checkpoint**: User Story 1 provides independently testable graph projection behavior.

---

## Phase 4: User Story 2 - Explore a Device-Centered Subgraph (Priority: P1)

**Goal**: Return bounded graph neighborhoods for authenticated callers with depth/limit clamping and truncation signal.

**Independent Test**: Query with varied depth/limit, authenticated/unauthenticated contexts, and verify response contract plus truncation behavior.

### Tests for User Story 2

- [ ] T020 [P] [US2] Add contract test for `GET /api/graph/devices` response shape in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesContractTests.cs`
- [ ] T021 [P] [US2] Add integration test for depth/limit clamping and truncation flag in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphQueryBoundingTests.cs`
- [ ] T022 [P] [US2] Add integration test for authenticated access requirement and unauthenticated rejection in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphDevicesAuthorizationTests.cs`

### Implementation for User Story 2

- [ ] T023 [US2] Implement graph query use case with depth/limit enforcement in `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`
- [ ] T024 [US2] Implement graph query repository adapter in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`
- [ ] T025 [US2] Implement graph endpoint DTOs in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesRequestDto.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/GraphDevicesResponseDto.cs`
- [ ] T026 [US2] Implement `GET /api/graph/devices` endpoint mapping/auth policy in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [ ] T027 [US2] Add endpoint/query documentation and rationale comments (Articles 29-31) in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs` and `src/NetworkMonitoring.Backend/Application/UseCases/GetDeviceGraphUseCase.cs`

**Checkpoint**: User Stories 1 and 2 are independently functional with bounded authenticated retrieval.

---

## Phase 5: User Story 3 - Enforce Graph Retention Hygiene (Priority: P2)

**Goal**: Run scheduled-only retention that removes stale relationships and orphan external hosts while preserving internal devices.

**Independent Test**: Seed stale relationships/orphan candidates and verify scheduled sweep removes only eligible targets; no manual trigger path exists.

### Tests for User Story 3

- [ ] T028 [P] [US3] Add integration test for stale relationship pruning in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionEdgePruningTests.cs`
- [ ] T029 [P] [US3] Add integration test for orphan external-host cleanup and internal-device preservation in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphRetentionNodeCleanupTests.cs`
- [ ] T030 [P] [US3] Add integration test asserting no manual on-demand retention trigger path in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphRetentionManualTriggerAbsenceTests.cs`

### Implementation for User Story 3

- [ ] T031 [US3] Implement retention sweep use case/outcome modeling in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`
- [ ] T032 [US3] Implement retention repository operations in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [ ] T033 [US3] Implement hosted retention scheduler (24h cadence only) in `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [ ] T034 [US3] Wire retention hosted service and options in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T035 [US3] Add retention API/boundary documentation and complex-flow rationale (Articles 29-31) in `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs` and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`

**Checkpoint**: User Stories 1-3 are independently testable with retention hygiene behavior.

---

## Phase 6: User Story 4 - Graceful Degradation on Graph Store Outage (Priority: P2)

**Goal**: Ensure graph-store failure returns explicit `503` for graph endpoint without affecting existing non-graph endpoints.

**Independent Test**: Simulate graph-store outage and verify graph endpoint fails gracefully while existing device/session endpoints remain healthy.

### Tests for User Story 4

- [ ] T036 [P] [US4] Add integration test for graph unavailable -> `503` contract in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphUnavailableContractTests.cs`
- [ ] T037 [P] [US4] Add integration test proving non-graph endpoint continuity during graph outage in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/GraphOutageIsolationTests.cs`

### Implementation for User Story 4

- [ ] T038 [US4] Implement endpoint-level unavailable mapping and payload in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [ ] T039 [US4] Implement infrastructure error translation for connectivity/timeouts in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphErrorMapper.cs`
- [ ] T040 [US4] Add registration guardrails so graph failures do not alter existing endpoint wiring in `src/NetworkMonitoring.Backend/Program.cs` and `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`

**Checkpoint**: All user stories independently functional and outage isolation validated.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Consolidate observability, measurable criteria, constitution compliance, and final readiness checks.

- [ ] T041 [P] Implement structured logs plus counters/latency metrics for projection and retention (FR-017) in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs` and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [ ] T042 Add integration test for observability baseline signals (SC-006) in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphObservabilitySignalsTests.cs`
- [ ] T043 Run graph-focused verification suite with explicit pass/fail gates (`dotnet test src/NetworkMonitoring.sln --filter "FullyQualifiedName~Graph"` + SC-001 median latency check <= 2s) and record outcomes in `specs/007-device-communication-graph/quickstart.md`
- [ ] T044 [P] Execute quickstart outage-isolation + auth-policy workflow and capture evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T045 Verify constitutional compliance record in `specs/007-device-communication-graph/plan.md`, including explicit SeedWork immutability check for `src/NetworkMonitoring.Domain/SeedWork/` per Article 21

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2 and uses US1 graph data for realistic retrieval validation.
- **Phase 5 (US3)**: Depends on Phase 2 and graph model from US1.
- **Phase 6 (US4)**: Depends on Phase 4 endpoint path and foundational error mapping.
- **Phase 7 (Polish)**: Depends on all targeted user stories complete.

### User Story Dependency Graph

- **US1 (P1)**: MVP slice; no user-story dependencies.
- **US2 (P1)**: Requires foundational abstractions; validation benefits from US1 projected data.
- **US3 (P2)**: Requires communication relationship model from US1.
- **US4 (P2)**: Requires graph endpoint from US2.

### Parallel Opportunities

- Setup tasks marked `[P]`: T003, T004.
- Foundational parallel task: T010.
- US1 parallel tests: T011, T012, T013.
- US2 parallel tests: T020, T021, T022.
- US3 parallel tests: T028, T029, T030.
- US4 parallel tests: T036, T037.
- Polish parallel tasks: T041, T044.

---

## Parallel Example: User Story 2

```bash
# Run US2 test tasks in parallel:
Task: "T020 [US2] ...GraphDevicesContractTests.cs"
Task: "T021 [US2] ...GraphQueryBoundingTests.cs"
Task: "T022 [US2] ...GraphDevicesAuthorizationTests.cs"

# Then implement independent endpoint DTO/query adapter parts in parallel:
Task: "T024 [US2] ...Neo4jGraphQueryRepository.cs"
Task: "T025 [US2] ...GraphDevicesRequestDto.cs / GraphDevicesResponseDto.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) as MVP.
3. Validate projection identity/idempotency/retry behavior before expanding scope.

### Incremental Delivery

1. Add US2 for bounded authenticated graph retrieval.
2. Add US3 for scheduled-only retention hygiene.
3. Add US4 for outage isolation hardening.
4. Finish with Phase 7 observability and measurable criteria validation.

### Suggested MVP Scope

- **MVP**: Phase 1 + Phase 2 + Phase 3 (US1 only).
- Provides immediate value by making communication graph projection available and testable.

---

## Notes

- All tasks follow required checklist format: checkbox, Task ID, optional `[P]`, required `[USx]` for story tasks, and explicit file path.
- Story tasks preserve independent testability and align with clarified FR-014/015/016/017 and SC-006.
