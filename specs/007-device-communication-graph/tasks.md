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
- [X] T003 [P] Add connector baseline for `sessions.enriched` projection in `infrastructure/connectors/configs/neo4j-sink-sessions-enriched.json`
- [X] T004 [P] Align quickstart + contract with defaults/error payload/auth clarifications in `specs/007-device-communication-graph/quickstart.md` and `specs/007-device-communication-graph/contracts/graph-api.md`

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

**Checkpoint**: All backend user stories independently functional and outage isolation validated.

---

## Phase 7: Runtime hardening - Neo4j default provider (post-baseline alignment)

**Purpose**: Align runtime behavior with approved move from in-memory graph storage to Neo4j in non-testing environments.

- [X] T041 Add runtime provider configuration (`Provider`, `Neo4jUri`, `Neo4jUsername`, `Neo4jPassword`) in `src/NetworkMonitoring.Backend/Application/Configuration/BackendOptions.cs` and `src/NetworkMonitoring.Backend/appsettings.json`
- [X] T042 Add Neo4j service to reference stack and backend runtime env wiring in `docker-compose.reference-stack.yml`
- [X] T043 Add Neo4j driver dependency and runtime native library requirement in `src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj` and `src/NetworkMonitoring.Backend/Dockerfile`
- [X] T044 Implement provider-aware repository registration (Neo4j default, InMemory for testing/fallback) in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/InMemoryGraphRepositories.cs`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jDriverAccessor.cs`
- [X] T045 Implement Neo4j-backed projection/query/retention repository behavior in `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphProjectionRepository.cs`, `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphQueryRepository.cs`, and `src/NetworkMonitoring.Backend/Infrastructure/Graph/Neo4jGraphRetentionRepository.cs`
- [X] T046 Add graph endpoint error logging for Neo4j-runtime diagnostics in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`

---

## Phase 8: User Story 5 - Visualize Graph Results in the UI (Priority: P1)

**Goal**: Add an operator-facing graph exploration page that consumes `GET /api/graph/devices` and renders nodes/edges/truncation plus resilient error states.

**Independent Test**: Use the UI to load graph data for a valid root identity, verify rendering and truncation, and validate `400`/`401`/`403`/`503` handling without full-page failure.

### Tests for User Story 5

- [ ] T047 [P] [US5] Add API client tests for graph retrieval success and error mapping in `src/NetworkMonitoring.Frontend/src/api/graphApi.test.ts`
- [ ] T048 [P] [US5] Add page tests covering load success, refresh failure preserving previous data, and auth/unavailable messages in `src/NetworkMonitoring.Frontend/src/pages/DeviceGraphPage.test.tsx`

### Implementation for User Story 5

- [ ] T049 [US5] Implement graph API client and response/error models in `src/NetworkMonitoring.Frontend/src/api/graphApi.ts` and `src/NetworkMonitoring.Frontend/src/models/graphDtos.ts`
- [ ] T050 [US5] Implement graph exploration page with query controls (`rootDeviceId`, `depth`, `limit`) and state machine in `src/NetworkMonitoring.Frontend/src/pages/DeviceGraphPage.tsx`
- [ ] T051 [US5] Implement visual graph component (nodes/edges interactive rendering) and tabular detail views in `src/NetworkMonitoring.Frontend/src/components/DeviceGraphVisualization.tsx` and `src/NetworkMonitoring.Frontend/src/components/DeviceGraphTable.tsx`
- [ ] T052 [US5] Wire app-level navigation to expose graph page while preserving existing inventory page in `src/NetworkMonitoring.Frontend/src/App.tsx`
- [ ] T053 [US5] Add TSDoc and rationale comments for new frontend exported components/functions and tests per constitution Articles 29-31

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, observability baseline, constitutional compliance, and release readiness.

- [X] T054 [P] Implement structured logs and counters/latency metrics for projection + retention in `src/NetworkMonitoring.Backend/Application/UseCases/ProjectCommunicationGraphUseCase.cs`, `src/NetworkMonitoring.Backend/Application/UseCases/RunGraphRetentionSweepUseCase.cs`, and `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [X] T055 Add integration test for observability baseline signals in `tests/NetworkMonitoring.Backend.IntegrationTests/Graph/GraphObservabilitySignalsTests.cs`
- [X] T056 Run graph-focused verification suite (`dotnet test tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj --filter "FullyQualifiedName~Graph"`) and record SC-001 latency evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T057 [P] Execute end-to-end UI + backend graph walkthrough and capture evidence in `specs/007-device-communication-graph/quickstart.md`
- [ ] T058 Verify constitutional compliance in `specs/007-device-communication-graph/plan.md`, including explicit SeedWork immutability check for `src/NetworkMonitoring.Domain/SeedWork/` (Article 21)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2; validation benefits from US1-projected data.
- **Phase 5 (US3)**: Depends on Phase 2 and graph model produced by US1 behavior.
- **Phase 6 (US4)**: Depends on Phase 4 endpoint path and foundational error mapping.
- **Phase 7 (Neo4j runtime hardening)**: Depends on Phase 2 and explicit maintainer confirmation for behavior shift.
- **Phase 8 (US5 UI)**: Depends on Phase 4 contract stability and Phase 7 runtime-ready graph source.
- **Phase 9 (Polish)**: Depends on all targeted user stories complete.

### User Story Dependency Graph

- **US1 (P1)**: MVP slice; no user-story dependency.
- **US2 (P1)**: Requires foundational graph abstractions.
- **US3 (P2)**: Requires foundational graph abstractions and retention model.
- **US4 (P2)**: Requires graph endpoint from US2 and error mapping foundation.
- **US5 (P1)**: Requires graph endpoint contract from US2 and runtime graph source from Phase 7.

