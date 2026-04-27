# Tasks: Device Inventory

**Input**: Design documents from `/specs/005-device-inventory/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are required by the feature prompt and specification. Unit, contract, persistence, architecture, and integration tests should be added before or alongside implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the separate backend/API deployable, tests, base configuration, and packaging.

- [x] T001 Create `src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj` with .NET 10 ASP.NET Core, EF Core PostgreSQL provider, Options, logging, and `src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj` reference
- [x] T002 Create `tests/NetworkMonitoring.Backend.UnitTests/NetworkMonitoring.Backend.UnitTests.csproj` referencing `src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj`
- [x] T003 Create `tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj` referencing `src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj`
- [x] T004 Add `NetworkMonitoring.Backend` and backend test projects to `src/NetworkMonitoring.sln`
- [x] T005 [P] Create backend host entry point in `src/NetworkMonitoring.Backend/Program.cs`
- [x] T006 [P] Create base configuration in `src/NetworkMonitoring.Backend/appsettings.json`
- [x] T007 [P] Add backend container packaging in `src/NetworkMonitoring.Backend/Dockerfile`
- [x] T008 [P] Add PostgreSQL local dependency documentation placeholder in `specs/005-device-inventory/quickstart.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define core Application models, ports, persistence mappings, host wiring, and architecture guardrails used by all stories.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T009 [P] Add backend configuration options in `src/NetworkMonitoring.Backend/Application/Configuration/BackendOptions.cs`
- [x] T010 [P] Add intake command model in `src/NetworkMonitoring.Backend/Application/Models/DeviceIntakeCommand.cs`
- [x] T011 [P] Add inventory read model in `src/NetworkMonitoring.Backend/Application/Models/DeviceInventoryItem.cs`
- [x] T012 [P] Add intake outcome model in `src/NetworkMonitoring.Backend/Application/Models/DeviceIntakeOutcome.cs`
- [x] T013 [P] Add repository port in `src/NetworkMonitoring.Backend/Application/Ports/IDeviceInventoryRepository.cs`
- [x] T014 [P] Add unit-of-work/transaction port in `src/NetworkMonitoring.Backend/Application/Ports/IInventoryUnitOfWork.cs`
- [x] T015 [P] Add clock port in `src/NetworkMonitoring.Backend/Application/Ports/IClock.cs`
- [x] T016 Add intake use case skeleton in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`
- [x] T017 Add inventory listing use case skeleton in `src/NetworkMonitoring.Backend/Application/UseCases/ListDevicesUseCase.cs`
- [x] T018 Add persistence entities in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/DeviceInventoryRecord.cs`
- [x] T019 Add EF Core DbContext in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/DeviceInventoryDbContext.cs`
- [x] T020 Add persistence mapping configuration with normalized MAC uniqueness and EF Core migration support in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/DeviceInventoryDbContext.cs`
- [x] T021 Add repository implementation skeleton in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/EfDeviceInventoryRepository.cs`
- [x] T022 Add host dependency injection wiring in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T023 Add endpoint route registration skeleton in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T024 [P] Add architecture dependency tests in `tests/NetworkMonitoring.Backend.UnitTests/Architecture/BackendArchitectureTests.cs`
- [x] T025 [P] Add shared-domain usage guardrail tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/SharedDomainUsageTests.cs`
- [x] T026 Add SeedWork immutability note and architecture check references to `specs/005-device-inventory/research.md`

**Checkpoint**: Backend foundation is ready; user stories can be implemented against ports and shared domain usage is enforced.

---

## Phase 3: User Story 1 - Accept Device Intake (Priority: P1) MVP

**Goal**: Accept a valid `POST /devices` request, actively validate it through shared domain types, store one device inventory record, and return a successful intake outcome.

**Independent Test**: Submit one valid intake request directly to the backend and verify the device is accepted, normalized, stored, and visible through diagnostics without probe, Kafka, Integration Console, or UI.

### Tests for User Story 1

- [x] T027 [P] [US1] Add shared-domain normalization tests for valid intake in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/AcceptDeviceIntakeUseCaseTests.cs`
- [x] T028 [P] [US1] Add `POST /devices` success contract tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceIntakeContractTests.cs`
- [x] T029 [P] [US1] Add PostgreSQL persistence create and migration-application tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Persistence/DeviceInventoryPersistenceTests.cs`
- [x] T030 [P] [US1] Add diagnostics tests for accepted intake in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeDiagnosticsTests.cs`

### Implementation for User Story 1

- [x] T031 [US1] Implement request DTOs for `POST /devices` in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceIntakeRequestDto.cs`
- [x] T032 [US1] Implement DTO-to-command mapper using `MacAddress`, `IpAddress`, and `DiscoverySource` in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceIntakeRequestMapper.cs`
- [x] T033 [US1] Implement `AcceptDeviceIntakeUseCase` creation path with shared `Device.Create` in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`
- [x] T034 [US1] Implement EF repository create path in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/EfDeviceInventoryRepository.cs`
- [x] T035 [US1] Implement persistence mapping between shared `Device` and `DeviceInventoryRecord` in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/DeviceInventoryMapper.cs`
- [x] T036 [US1] Implement `POST /devices` endpoint and success response mapping in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T037 [US1] Register DbContext, repository, use case, and endpoint services in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T038 [US1] Add accepted-intake structured logging in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`

**Checkpoint**: US1 accepts and stores one valid device through `POST /devices` and can be validated without any other service.

---

## Phase 4: User Story 2 - Preserve Idempotent Device State (Priority: P1)

**Goal**: Repeated or retry intake for the same normalized MAC updates/confirms the existing device without duplicate stored records.

**Independent Test**: Submit the same valid intake request multiple times and verify exactly one stored device identity exists while duplicate/idempotent diagnostics are recorded.

### Tests for User Story 2

- [x] T039 [P] [US2] Add idempotency, timestamp consolidation, unique observed IP, hostname update, and primary IP update tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeIdempotencyTests.cs`
- [x] T040 [P] [US2] Add unique normalized MAC persistence tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Persistence/DeviceInventoryUniquenessTests.cs`
- [x] T041 [P] [US2] Add concurrent duplicate intake tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceIntakeConcurrencyTests.cs`
- [x] T042 [P] [US2] Add duplicate/idempotent diagnostics tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeDiagnosticsTests.cs`

### Implementation for User Story 2

- [x] T043 [US2] Implement repository lookup by normalized MAC in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/EfDeviceInventoryRepository.cs`
- [x] T044 [US2] Implement idempotent existing-device branch with earliest `firstSeenUtc`, latest `lastSeenUtc`, unique `observedIps`, and deterministic non-null `hostname`/`primaryIp` update rules in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`
- [x] T045 [US2] Enforce database unique constraint for normalized MAC in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/DeviceInventoryDbContext.cs`
- [x] T046 [US2] Map duplicate/idempotent outcome to HTTP success response in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T047 [US2] Add duplicate/idempotent structured logging in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`

**Checkpoint**: US2 proves repeated intake is safe and one logical device exists per normalized MAC.

---

## Phase 5: User Story 3 - Reject Invalid Intake Safely (Priority: P1)

**Goal**: Reject malformed, identity-ambiguous, invalid, or persistence-failed intake without changing stored device state.

**Independent Test**: Submit invalid intake requests directly to the backend and verify clear rejection outcomes, diagnostics, and unchanged inventory state.

### Tests for User Story 3

- [x] T048 [P] [US3] Add missing/malformed `Idempotency-Key` tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceIntakeValidationTests.cs`
- [x] T049 [P] [US3] Add MAC mismatch validation tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeValidationTests.cs`
- [x] T050 [P] [US3] Add invalid MAC/IP and required-field tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeValidationTests.cs`
- [x] T051 [P] [US3] Add invalid timestamp ordering tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakeValidationTests.cs`
- [x] T052 [P] [US3] Add malformed body and unsupported content-type contract tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceIntakeValidationTests.cs`
- [x] T053 [P] [US3] Add persistence failure handling tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/DeviceIntakePersistenceFailureTests.cs`

### Implementation for User Story 3

- [x] T054 [US3] Implement boundary validation for `Idempotency-Key`, body MAC, malformed body, and content type in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T055 [US3] Implement Application validation using shared `MacAddress`, `IpAddress`, `DiscoverySource`, and `Device` invariants in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`
- [x] T056 [US3] Implement rejected outcome model mapping in `src/NetworkMonitoring.Backend/Application/Models/DeviceIntakeOutcome.cs`
- [x] T057 [US3] Map validation failures to HTTP rejection responses in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T058 [US3] Ensure repository/unit-of-work failures do not partially update inventory in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/EfDeviceInventoryRepository.cs`
- [x] T059 [US3] Add rejected and persistence-failure structured logging in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`

**Checkpoint**: US3 rejects bad intake safely and preserves existing inventory state.

---

## Phase 6: User Story 4 - Query Stored Devices (Priority: P2)

**Goal**: Expose `GET /devices` to return consolidated inventory state for validation and future UI use.

**Independent Test**: Store zero, one, and multiple devices through intake, then request the inventory list and verify one item per normalized MAC with consolidated key fields.

### Tests for User Story 4

- [x] T060 [P] [US4] Add empty inventory query contract tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceInventoryQueryContractTests.cs`
- [x] T061 [P] [US4] Add populated inventory query contract tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceInventoryQueryContractTests.cs`
- [x] T062 [P] [US4] Add read model mapping tests in `tests/NetworkMonitoring.Backend.UnitTests/Application/UseCases/ListDevicesUseCaseTests.cs`
- [x] T063 [P] [US4] Add read-only query behavior tests in `tests/NetworkMonitoring.Backend.IntegrationTests/Api/DeviceInventoryQueryContractTests.cs`

### Implementation for User Story 4

- [x] T064 [US4] Implement list repository method in `src/NetworkMonitoring.Backend/Infrastructure/Persistence/EfDeviceInventoryRepository.cs`
- [x] T065 [US4] Implement `ListDevicesUseCase` in `src/NetworkMonitoring.Backend/Application/UseCases/ListDevicesUseCase.cs`
- [x] T066 [US4] Implement response DTOs for `GET /devices` in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceInventoryResponseDto.cs`
- [x] T067 [US4] Implement `GET /devices` endpoint in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`
- [x] T068 [US4] Add inventory query diagnostics in `src/NetworkMonitoring.Backend/Host/Endpoints/DeviceEndpoints.cs`

**Checkpoint**: US4 provides a UI-ready inventory list without implementing the UI.

---

## Phase 7: User Story 5 - Validate End-to-End Backend Handoff (Priority: P2)

**Goal**: Prove the Integration Console can forward to the real backend and backend persistence remains idempotent.

**Independent Test**: Run backend and Integration Console together with controlled events/requests and verify `GET /devices` shows exactly one stored device for the normalized MAC.

### Tests for User Story 5

- [x] T069 [P] [US5] Add Integration Console forwarding-to-backend test in `tests/NetworkMonitoring.Backend.IntegrationTests/IntegrationConsole/IntegrationConsoleForwardingToBackendTests.cs`
- [x] T070 [P] [US5] Add retry/idempotency end-to-end test in `tests/NetworkMonitoring.Backend.IntegrationTests/IntegrationConsole/IntegrationConsoleBackendIdempotencyTests.cs`
- [x] T071 [P] [US5] Add backend + PostgreSQL test host fixture in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/BackendTestApplicationFactory.cs`

### Implementation for User Story 5

- [x] T072 [US5] Add backend test server support utilities in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/BackendTestApplicationFactory.cs`
- [x] T073 [US5] Add Integration Console test configuration helper pointing `BackendBaseUrl` to real backend in `tests/NetworkMonitoring.Backend.IntegrationTests/Support/IntegrationConsoleBackendHarness.cs`
- [x] T074 [US5] Document Integration Console to real backend validation in `specs/005-device-inventory/quickstart.md`

**Checkpoint**: US5 proves the 004 -> 005 handoff without changing Integration Console Kafka consumption.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full feature, update documentation, and keep repository artifacts consistent.

- [x] T075 [P] Add PostgreSQL local run guidance and migration/application initialization guidance to `specs/005-device-inventory/quickstart.md`
- [x] T076 [P] Update `README.md` with Device Inventory backend run and validation pointers to `specs/005-device-inventory/quickstart.md`
- [x] T077 [P] Update `docker-compose.reference-stack.yml` only if needed to add PostgreSQL/backend validation without changing existing probe/Kafka behavior
- [x] T078 Add `NetworkMonitoring.Backend` Docker build validation command/result to `specs/005-device-inventory/quickstart.md`
- [x] T079 Run `dotnet test src/NetworkMonitoring.sln` and record result in `specs/005-device-inventory/research.md`
- [x] T080 Run or document PostgreSQL-backed integration validation and record outcome in `specs/005-device-inventory/research.md`
- [x] T081 Run architecture consistency pass confirming Application has no Infrastructure dependency in `tests/NetworkMonitoring.Backend.UnitTests/Architecture/BackendArchitectureTests.cs`
- [x] T082 Confirm no changes were made to probe-side discovery, `DeviceDetected`, or Integration Console Kafka consumption in `src/NetworkMonitoring.Probe/` and `src/NetworkMonitoring.IntegrationConsole/`
- [x] T083 Run cross-artifact consistency pass for `specs/005-device-inventory/spec.md`, `plan.md`, `tasks.md`, `data-model.md`, `contracts/`, and `quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start here.
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories.
- **US1 Accept Device Intake (Phase 3)**: Depends on Foundational; MVP backend write path.
- **US2 Preserve Idempotent Device State (Phase 4)**: Depends on US1 write path and repository primitives.
- **US3 Reject Invalid Intake Safely (Phase 5)**: Depends on US1 validation/write path; can be developed alongside US2 after foundation, but should be validated before broad integration.
- **US4 Query Stored Devices (Phase 6)**: Depends on persistence foundation and benefits from US1 stored data.
- **US5 End-to-End Backend Handoff (Phase 7)**: Depends on US1, US2, and US4.
- **Polish (Phase 8)**: Depends on desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Independent MVP after Foundational.
- **US2 (P1)**: Extends US1 to repeated/duplicate intake.
- **US3 (P1)**: Independent validation slice after Foundational and US1 DTO/use-case shape.
- **US4 (P2)**: Reads persisted data; depends on persistence foundation and US1 for meaningful populated tests.
- **US5 (P2)**: Cross-component validation; depends on backend intake, idempotency, and query behavior.

### Within Each User Story

- Tests are added before or alongside implementation.
- Application models/ports precede use cases.
- Use cases precede endpoint implementation.
- Persistence mappings/repository methods precede end-to-end integration tests.
- Story checkpoints should pass before moving to dependent stories.

### Parallel Opportunities

- T005, T006, T007, and T008 can run in parallel after project files are created.
- T009 through T015 can run in parallel during Foundational work.
- T024 and T025 can run in parallel with foundational skeletons.
- T027 through T030 can run in parallel for US1 tests.
- T039 through T042 can run in parallel for US2 tests.
- T048 through T053 can run in parallel for US3 tests.
- T060 through T063 can run in parallel for US4 tests.
- T069 through T071 can run in parallel for US5 tests.
- T075 through T077 can run in parallel during Polish.

---

## Parallel Example: User Story 1

```bash
Task: "T027 [US1] Add shared-domain normalization tests"
Task: "T028 [US1] Add POST /devices success contract tests"
Task: "T029 [US1] Add PostgreSQL persistence create tests"
Task: "T030 [US1] Add diagnostics tests for accepted intake"
```

---

## Parallel Example: User Story 3

```bash
Task: "T048 [US3] Add missing/malformed Idempotency-Key tests"
Task: "T049 [US3] Add MAC mismatch validation tests"
Task: "T050 [US3] Add invalid MAC/IP and required-field tests"
Task: "T051 [US3] Add invalid timestamp ordering tests"
Task: "T052 [US3] Add malformed body and unsupported content-type contract tests"
Task: "T053 [US3] Add persistence failure handling tests"
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 US1 to accept and persist one valid device intake request.
4. Stop and validate US1 independently with direct HTTP/API and persistence tests.

### Incremental Delivery

1. US1: Accept and persist valid intake.
2. US2: Add idempotency and duplicate prevention.
3. US3: Harden validation and rejection behavior.
4. US4: Add inventory listing for future UI.
5. US5: Prove Integration Console forwarding to real backend.
6. Polish: Docker, docs, full test suite, PostgreSQL validation, and consistency checks.

### Scope Guardrails

- Do not implement UI, login/RBAC, session backend behavior, or new user-facing management screens.
- Do not change probe-side discovery or `DeviceDetected` publication.
- Do not redefine the `DeviceDetected` Kafka contract.
- Do not change Integration Console Kafka consumption behavior unless required by `POST /devices` compatibility.
- Do not create backend-only duplicate domain entities for `Device`, `MacAddress`, `IpAddress`, or `DiscoverySource`.
- Keep Application dependent on shared Domain abstractions and ports only; Infrastructure implements Application ports.

---

## Notes

- **Total tasks**: 83.
- **US1 tasks**: 12 (T027-T038).
- **US2 tasks**: 9 (T039-T047).
- **US3 tasks**: 12 (T048-T059).
- **US4 tasks**: 9 (T060-T068).
- **US5 tasks**: 6 (T069-T074).
- **MVP scope**: Phase 1 + Phase 2 + US1.
