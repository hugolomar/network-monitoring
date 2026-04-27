# Tasks: Device Ingestion

**Input**: Design documents from `/specs/004-device-ingestion/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are required by the feature prompt and specification. Unit/contract tests should be added before or alongside implementation, and gated Kafka integration tests must stay opt-in with `RUN_KAFKA_INTEGRATION=1`.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the separate Integration Console deployable and test project skeleton.

- [x] T001 Create `src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj` with .NET 10 Worker, Kafka, Schema Registry Avro, Options, HttpClient, and shared domain references
- [x] T002 Create `tests/NetworkMonitoring.IntegrationConsole.UnitTests/NetworkMonitoring.IntegrationConsole.UnitTests.csproj` referencing `src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj`
- [x] T003 Create `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/NetworkMonitoring.IntegrationConsole.IntegrationTests.csproj` referencing `src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj`
- [x] T004 Add Integration Console and its test projects to `src/NetworkMonitoring.sln`
- [x] T005 [P] Create Integration Console host entry point in `src/NetworkMonitoring.IntegrationConsole/Program.cs`
- [x] T006 [P] Create base configuration in `src/NetworkMonitoring.IntegrationConsole/appsettings.json`
- [x] T007 [P] Add deployable container packaging in `src/NetworkMonitoring.IntegrationConsole/Dockerfile`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define shared models, configuration, ports, and host wiring used by all user stories.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T008 [P] Add `IntegrationConsoleOptions` in `src/NetworkMonitoring.IntegrationConsole/Application/Configuration/IntegrationConsoleOptions.cs`
- [x] T009 [P] Add consumed event model `DeviceDetectedEvent` in `src/NetworkMonitoring.IntegrationConsole/Application/Models/DeviceDetectedEvent.cs`
- [x] T010 [P] Add outbound request model `DeviceIntakeRequest` in `src/NetworkMonitoring.IntegrationConsole/Application/Models/DeviceIntakeRequest.cs`
- [x] T011 [P] Add ingestion outcome models in `src/NetworkMonitoring.IntegrationConsole/Application/Models/IngestionOutcome.cs`
- [x] T012 [P] Add event consumption port `IDeviceEventConsumer` in `src/NetworkMonitoring.IntegrationConsole/Application/Ports/IDeviceEventConsumer.cs`
- [x] T013 [P] Add device intake forwarding port `IDeviceIntakeClient` in `src/NetworkMonitoring.IntegrationConsole/Application/Ports/IDeviceIntakeClient.cs`
- [x] T014 Add ingestion use case skeleton in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`
- [x] T015 Add worker service skeleton in `src/NetworkMonitoring.IntegrationConsole/Host/Services/IntegrationConsoleWorker.cs`
- [x] T016 Add dependency injection wiring in `src/NetworkMonitoring.IntegrationConsole/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T017 Add SeedWork immutability guardrail note to `specs/004-device-ingestion/research.md`

**Checkpoint**: Foundation ready; each user story can now be implemented against ports and models.

---

## Phase 3: User Story 1 - Consume Device Detections (Priority: P1)

**Goal**: Consume `DeviceDetected` events from `devices.detected`, decode Avro values, preserve normalized MAC identity, and classify malformed or identity-ambiguous events without requiring the real backend.

**Independent Test**: With a controlled event stream or fake consumer containing one valid `DeviceDetected`, the Integration Console accepts the event, validates key/payload MAC identity, and records processing progress without forwarding to a real backend.

### Tests for User Story 1

- [x] T018 [P] [US1] Add Avro/GenericRecord mapping tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Infrastructure/Serialization/DeviceDetectedEventMapperTests.cs`
- [x] T019 [P] [US1] Add event identity validation tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/DeviceEventValidationTests.cs`
- [x] T020 [P] [US1] Add fake-consumer use case test for valid event progress in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/ProcessDeviceDetectionsUseCaseConsumeTests.cs`
- [x] T021 [P] [US1] Add malformed/deserialization failure classification tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/RejectedEventTests.cs`

### Implementation for User Story 1

- [x] T022 [US1] Implement `DeviceDetectedEventMapper` in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Serialization/DeviceDetectedEventMapper.cs`
- [x] T023 [US1] Implement device event validation in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`
- [x] T024 [US1] Implement Kafka Avro consumer adapter and processing-position acknowledgement semantics in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Ingestion/KafkaDeviceEventConsumer.cs`
- [x] T025 [US1] Register Kafka consumer and mapper dependencies in `src/NetworkMonitoring.IntegrationConsole/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T026 [US1] Add operator-visible logging for consumed and rejected events in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`

**Checkpoint**: US1 is independently testable with fake consumer inputs and can decode/validate `DeviceDetected` events without a real backend.

---

## Phase 4: User Story 2 - Forward Valid Devices (Priority: P1)

**Goal**: Map valid consumed events to the `POST /devices` contract and send them to a configurable fake/test HTTP receiver.

**Independent Test**: With a valid `DeviceDetected` event and fake HTTP receiver, the Integration Console sends exactly one `POST /devices` request with the expected JSON body and normalized MAC idempotency identity.

### Tests for User Story 2

- [x] T027 [P] [US2] Add DeviceDetected-to-intake mapping tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Infrastructure/Backend/DeviceIntakeRequestMapperTests.cs`
- [x] T028 [P] [US2] Add HTTP contract tests for path, JSON body, and `Idempotency-Key` in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Infrastructure/Backend/HttpDeviceIntakeClientTests.cs`
- [x] T029 [P] [US2] Add fake receiver forwarding integration tests in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/FakeDeviceReceiverForwardingTests.cs`

### Implementation for User Story 2

- [x] T030 [US2] Implement `DeviceIntakeRequestMapper` in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Backend/DeviceIntakeRequestMapper.cs`
- [x] T031 [US2] Implement `HttpDeviceIntakeClient` in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Backend/HttpDeviceIntakeClient.cs`
- [x] T032 [US2] Integrate `IDeviceIntakeClient` forwarding and successful forwarding diagnostics in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`
- [x] T033 [US2] Register backend HTTP client and options in `src/NetworkMonitoring.IntegrationConsole/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T034 [US2] Add fake/test receiver helper in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/Support/FakeDeviceReceiver.cs`

**Checkpoint**: US2 forwards a valid event to fake `POST /devices` and preserves the HTTP contract without implementing the real backend.

---

## Phase 5: User Story 3 - Recover From Delivery Failures (Priority: P2)

**Goal**: Retry transient backend/network failures, stop retrying permanent validation failures, and record clear diagnostics for retry exhaustion and rejected events.

**Independent Test**: With a fake receiver that returns transient failures before success, the Integration Console retries according to configured policy and either succeeds or records retry exhaustion.

### Tests for User Story 3

- [x] T035 [P] [US3] Add retry policy tests for 408, 429, 5xx, timeout, and network failures in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Infrastructure/Backend/DeviceIntakeRetryPolicyTests.cs`
- [x] T036 [P] [US3] Add permanent rejection tests for 400 and 422 responses in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/DeviceIntakeFailureClassificationTests.cs`
- [x] T037 [P] [US3] Add retry-exhaustion fake receiver integration tests in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/FakeDeviceReceiverRetryTests.cs`
- [x] T038 [P] [US3] Add poison-message continuation tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/PoisonMessageHandlingTests.cs`

### Implementation for User Story 3

- [x] T039 [US3] Implement retry policy model in `src/NetworkMonitoring.IntegrationConsole/Application/Configuration/RetryOptions.cs`
- [x] T040 [US3] Implement retry classification in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Backend/DeviceIntakeRetryPolicy.cs`
- [x] T041 [US3] Apply retry handling in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Backend/HttpDeviceIntakeClient.cs`
- [x] T042 [US3] Record retry-exhausted and rejected outcomes in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`
- [x] T043 [US3] Add retry/backoff configuration defaults in `src/NetworkMonitoring.IntegrationConsole/appsettings.json`

**Checkpoint**: US3 handles transient and permanent failures predictably and continues after poison/rejected events.

---

## Phase 6: User Story 4 - Preserve Idempotent Device Intake (Priority: P2)

**Goal**: Ensure duplicate or retried detections for the same normalized MAC carry the same idempotency identity and do not create duplicate downstream effects in fake receiver validation.

**Independent Test**: With duplicate `DeviceDetected` events for the same normalized MAC, requests carry the same `Idempotency-Key`, and the fake receiver observes no duplicate downstream device effect.

### Tests for User Story 4

- [x] T044 [P] [US4] Add idempotency header tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Infrastructure/Backend/DeviceIntakeIdempotencyTests.cs`
- [x] T045 [P] [US4] Add duplicate-event fake receiver tests in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/FakeDeviceReceiverIdempotencyTests.cs`
- [x] T046 [P] [US4] Add retry idempotency tests in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Application/UseCases/RetryIdempotencyTests.cs`

### Implementation for User Story 4

- [x] T047 [US4] Ensure `Idempotency-Key` is always normalized MAC in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Backend/HttpDeviceIntakeClient.cs`
- [x] T048 [US4] Ensure duplicate/retry attempts preserve correlation identity in `src/NetworkMonitoring.IntegrationConsole/Application/UseCases/ProcessDeviceDetectionsUseCase.cs`
- [x] T049 [US4] Add fake receiver duplicate-effect tracking in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/Support/FakeDeviceReceiver.cs`

**Checkpoint**: US4 proves repeated detections and retries are safe for the future backend idempotency contract.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the full feature, update docs, and keep repository artifacts consistent.

- [x] T050 [P] Add gated Kafka consume/decode integration test in `tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/KafkaDeviceIngestionIntegrationTests.cs`
- [x] T051 [P] Update `README.md` with Integration Console run and validation pointers to `specs/004-device-ingestion/quickstart.md`
- [x] T052 [P] Update `docker-compose.reference-stack.yml` only if needed to document/run the Integration Console container without changing existing probe/Kafka behavior
- [x] T053 Run `dotnet test src/NetworkMonitoring.sln` and record results in `specs/004-device-ingestion/research.md`
- [x] T054 Run or document gated `RUN_KAFKA_INTEGRATION=1` validation and record outcome in `specs/004-device-ingestion/research.md`
- [x] T055 Validate `src/NetworkMonitoring.IntegrationConsole/Dockerfile` builds and record outcome in `specs/004-device-ingestion/quickstart.md`
- [x] T056 Run cross-artifact consistency pass for `specs/004-device-ingestion/spec.md`, `plan.md`, `tasks.md`, `data-model.md`, `contracts/`, and `quickstart.md`
- [x] T057 Confirm no changes were made to probe-side discovery/Kafka publication in `src/NetworkMonitoring.Probe/` beyond solution wiring if required

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start here.
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; MVP event consumption and validation.
- **User Story 2 (Phase 4)**: Depends on Foundational and can use fake inputs, but full end-to-end flow benefits from US1.
- **User Story 3 (Phase 5)**: Depends on US2 HTTP forwarding primitives.
- **User Story 4 (Phase 6)**: Depends on US2 HTTP forwarding primitives and US3 retry behavior for retry-idempotency checks.
- **Polish (Phase 7)**: Depends on desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Independent after Foundational; validates event consumption and rejection classification.
- **US2 (P1)**: Independent after Foundational with fake events; integrates with US1 for full consume-forward flow.
- **US3 (P2)**: Requires the `IDeviceIntakeClient`/HTTP forwarding path from US2.
- **US4 (P2)**: Requires the HTTP forwarding path from US2 and retry semantics from US3 for retry idempotency.

### Within Each User Story

- Tests are added before or alongside implementation.
- Models and ports precede use cases and infrastructure adapters.
- Infrastructure adapters are registered in DI after their interfaces and implementations exist.
- Story checkpoints should pass before moving to the next dependent phase.

### Parallel Opportunities

- T005, T006, and T007 can run in parallel after project files are created.
- T008 through T013 can run in parallel during Foundational work.
- T018 through T021 can run in parallel for US1 tests.
- T027 through T029 can run in parallel for US2 tests.
- T035 through T038 can run in parallel for US3 tests.
- T044 through T046 can run in parallel for US4 tests.
- T050, T051, and T052 can run in parallel during Polish.

---

## Parallel Example: User Story 1

```bash
Task: "T018 [US1] Add Avro/GenericRecord mapping tests"
Task: "T019 [US1] Add event identity validation tests"
Task: "T020 [US1] Add fake-consumer use case test"
Task: "T021 [US1] Add malformed/deserialization failure classification tests"
```

---

## Parallel Example: User Story 2

```bash
Task: "T027 [US2] Add DeviceDetected-to-intake mapping tests"
Task: "T028 [US2] Add HTTP contract tests"
Task: "T029 [US2] Add fake receiver forwarding integration tests"
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 US1 to consume/decode/validate `DeviceDetected` events without a real backend.
4. Stop and validate US1 independently with fake consumer inputs.

### Incremental Delivery

1. US1: Consume and validate events.
2. US2: Forward valid events to fake `POST /devices`.
3. US3: Add retry, rejection, and poison-message behavior.
4. US4: Prove idempotency identity for duplicates and retries.
5. Polish: gated Kafka validation, Docker packaging validation, docs, and consistency checks.

### Scope Guardrails

- Do not implement the real backend/API, persistence, `GET /devices`, UI, login, or RBAC.
- Do not change `DeviceDetected` producer behavior or probe-side discovery in `src/NetworkMonitoring.Probe/`.
- Preserve `devices.detected` and `devices.detected-value` as contracts owned by `003-device-discovery`.
- Keep Kafka, Schema Registry, HTTP, retries, and logging in Infrastructure behind Application ports.

---

## Notes

- **Total tasks**: 57.
- **US1 tasks**: 9 (T018-T026).
- **US2 tasks**: 8 (T027-T034).
- **US3 tasks**: 9 (T035-T043).
- **US4 tasks**: 6 (T044-T049).
- **MVP scope**: Phase 1 + Phase 2 + US1.
