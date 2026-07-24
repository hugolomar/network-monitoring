# Tasks: Production Observability Baseline

**Input**: Design documents from `/specs/008-observability-baseline/`  
**Prerequisites**: `plan.md` (required), `spec.md` (required), `research.md`, `data-model.md`, `contracts/`

**Tests**: Include automated tests and CI checks because objective verification is part of the feature
requirements (FR-006..FR-010, SC-001..SC-006, Article 36).

**Organization**: Tasks are grouped by user story so each story can be implemented and validated
independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Parallelizable task (different files, no unmet dependency)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`)
- All task descriptions include concrete file paths

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish observability baseline scaffolding and local stack wiring.

- [X] T001 Add observability baseline environment variables to `.env.example`
- [X] T002 Add observability compose services and wiring in `docker-compose.reference-stack.yml`
- [X] T003 [P] Add collector base configuration in `infrastructure/observability/otel-collector-config.yml`
- [X] T004 [P] Add Prometheus scrape baseline in `infrastructure/observability/prometheus.yml`
- [X] T005 [P] Add Grafana datasource provisioning in `infrastructure/observability/grafana/provisioning/datasources/datasources.yml`
- [X] T006 [P] Add Grafana dashboard provisioning in `infrastructure/observability/grafana/provisioning/dashboards/dashboards.yml`
- [X] T062 [P] Add Elasticsearch index template for structured logs in `infrastructure/observability/elasticsearch/logs-index-template.json`
- [X] T063 [P] Add Kibana saved search/dashboard bootstrap for log diagnostics in `infrastructure/observability/kibana/observability-logs.ndjson`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared primitives required before any story implementation starts.

**⚠️ CRITICAL**: Complete this phase before user-story phases.

- [X] T007 Create shared observability options model in `src/NetworkMonitoring.Backend/Application/Configuration/ObservabilityOptions.cs`
- [X] T008 [P] Add correlation context abstraction in `src/NetworkMonitoring.Domain/Abstractions/ICorrelationContext.cs`
- [X] T009 [P] Implement correlation propagation middleware in `src/NetworkMonitoring.Backend/Host/Middleware/CorrelationMiddleware.cs`
- [X] T010 [P] Add structured logging bootstrap extensions in `src/NetworkMonitoring.Backend/Host/DependencyInjection/LoggingExtensions.cs`
- [X] T011 [P] Add OpenTelemetry bootstrap extensions in `src/NetworkMonitoring.Backend/Host/DependencyInjection/TelemetryExtensions.cs`
- [X] T012 Add service health endpoint mapping baseline in `src/NetworkMonitoring.Backend/Program.cs`
- [X] T013 Add telemetry hygiene redaction utility in `src/NetworkMonitoring.Backend/Host/Telemetry/TelemetryRedaction.cs`
- [X] T014 [P] Add baseline observability contract section references in `README.md`
- [X] T015 Add CI gate script for observability validation in `infrastructure/ci/check-observability-baseline.sh`
- [X] T056 Add explicit SeedWork immutability check task in `infrastructure/ci/check-seedwork-immutability.sh`
- [X] T064 Add collector pipeline/exporter for Elasticsearch logs in `infrastructure/observability/otel-collector-config.yml`

**Checkpoint**: Shared observability foundation available for all user stories.

---

## Phase 3: User Story 1 - Diagnose Production Errors Quickly (Priority: P1) 🎯 MVP

**Goal**: Operators can diagnose failed operations quickly via correlated, structured, hygienic signals.

**Independent Test**: Trigger controlled failures and verify discoverability by correlation ID and
responsible component without server access.

### Tests for User Story 1

- [X] T016 [P] [US1] Add backend correlation propagation integration test in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/CorrelationPropagationTests.cs`
- [X] T017 [P] [US1] Add structured log shape test for backend in `tests/NetworkMonitoring.Backend.UnitTests/Observability/StructuredLoggingContractTests.cs`
- [X] T018 [P] [US1] Add telemetry redaction test (no PII/secrets) in `tests/NetworkMonitoring.Backend.UnitTests/Observability/TelemetryHygieneTests.cs`
- [X] T019 [P] [US1] Add probe structured log contract test in `tests/NetworkMonitoring.Probe.UnitTests/Observability/ProbeStructuredLoggingTests.cs`
- [X] T020 [P] [US1] Add integration-console structured log contract test in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Observability/IntegrationStructuredLoggingTests.cs`
- [X] T057 [P] [US1] Add frontend diagnostics correlation test in `src/NetworkMonitoring.Frontend/src/__tests__/diagnostics-correlation.test.ts`
- [X] T065 [P] [US1] Add integration test for correlation-based log retrieval from Elasticsearch in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/ElasticsearchLogCorrelationTests.cs`

### Implementation for User Story 1

- [X] T021 [US1] Wire correlation middleware into request pipeline in `src/NetworkMonitoring.Backend/Program.cs`
- [X] T022 [US1] Add correlation extraction/injection in `src/NetworkMonitoring.Backend/Host/Middleware/CorrelationMiddleware.cs`
- [X] T023 [US1] Add structured logging enrichment + OTLP log export wiring for backend in `src/NetworkMonitoring.Backend/Host/DependencyInjection/LoggingExtensions.cs`
- [X] T024 [US1] Add structured logging enrichment + OTLP log export wiring for probe in `src/NetworkMonitoring.Probe/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T025 [US1] Add structured logging enrichment + OTLP log export wiring for integration console in `src/NetworkMonitoring.IntegrationConsole/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T026 [US1] Apply telemetry redaction at log emission boundaries in `src/NetworkMonitoring.Backend/Host/Telemetry/TelemetryRedaction.cs`
- [X] T027 [US1] Add mandatory error context fields in `src/NetworkMonitoring.Backend/Host/Endpoints/GraphEndpoints.cs`
- [X] T028 [US1] Document diagnostic lookup workflow in `specs/008-observability-baseline/quickstart.md`
- [X] T058 [US1] Implement frontend diagnostic correlation lookup behavior in `src/NetworkMonitoring.Frontend/src/pages/DiagnosticsPage.tsx`
- [X] T066 [US1] Add Kibana log lookup guide for incident diagnosis in `specs/008-observability-baseline/quickstart.md`

**Checkpoint**: Error diagnosis baseline is independently functional and testable.

---

## Phase 4: User Story 2 - Trace Distributed Execution End-to-End (Priority: P2)

**Goal**: Distributed operations are traceable end-to-end across services and boundaries.

**Independent Test**: Execute cross-service flow and verify complete trace/correlation continuity.

### Tests for User Story 2

- [X] T029 [P] [US2] Add distributed trace continuity integration test in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/DistributedTraceContinuityTests.cs`
- [X] T030 [P] [US2] Add async boundary context propagation test in `tests/NetworkMonitoring.IntegrationConsole.UnitTests/Observability/AsyncPropagationTests.cs`
- [X] T031 [P] [US2] Add baseline metrics presence test in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/BaselineMetricsCoverageTests.cs`

### Implementation for User Story 2

- [X] T032 [US2] Enable backend OpenTelemetry traces and metrics in `src/NetworkMonitoring.Backend/Host/DependencyInjection/TelemetryExtensions.cs`
- [X] T033 [US2] Enable probe OpenTelemetry signals in `src/NetworkMonitoring.Probe/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T034 [US2] Enable integration-console OpenTelemetry signals in `src/NetworkMonitoring.IntegrationConsole/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T035 [US2] Implement async context propagation for Kafka consume/publish in `src/NetworkMonitoring.IntegrationConsole/Infrastructure/Ingestion/KafkaDeviceEventConsumer.cs`
- [X] T036 [US2] Implement backend trace linkage for intake/projection paths in `src/NetworkMonitoring.Backend/Application/UseCases/AcceptDeviceIntakeUseCase.cs`
- [X] T037 [US2] Replace no-op graph telemetry with OTel-backed adapter in `src/NetworkMonitoring.Backend/Infrastructure/Graph/NullGraphTelemetry.cs`
- [X] T038 [US2] Register OTel graph telemetry adapter in `src/NetworkMonitoring.Backend/Host/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T039 [US2] Add observability collector exporter configuration for traces/metrics/logs in `infrastructure/observability/otel-collector-config.yml`
- [X] T059 [US2] Add frontend distributed trace context rendering in `src/NetworkMonitoring.Frontend/src/components/TraceDetailsPanel.tsx`

**Checkpoint**: End-to-end traceability and baseline metrics are independently functional and testable.

---

## Phase 5: User Story 3 - Detect and Escalate Degradation Early (Priority: P3)

**Goal**: Critical-flow degradation triggers actionable alerts before severe user impact.

**Independent Test**: Simulate degradation and validate alert emission with diagnosis context.

### Tests for User Story 3

- [X] T040 [P] [US3] Add service health readiness/liveness integration test in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/ServiceHealthEndpointsTests.cs`
- [X] T041 [P] [US3] Add critical-flow alert trigger contract test in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/CriticalFlowAlertingTests.cs`
- [X] T042 [P] [US3] Add CI gate failure test for missing baseline obligations in `tests/NetworkMonitoring.Backend.UnitTests/Observability/ObservabilityGatePolicyTests.cs`
- [X] T067 [P] [US3] Add alert lead-time threshold validation test for SC-004 in `tests/NetworkMonitoring.Backend.IntegrationTests/Observability/AlertLeadTimeComplianceTests.cs`

### Implementation for User Story 3

- [X] T043 [US3] Add backend liveness/readiness checks in `src/NetworkMonitoring.Backend/Program.cs`
- [X] T044 [US3] Add probe service health exposure in `src/NetworkMonitoring.Probe/Program.cs`
- [X] T045 [US3] Add integration-console health exposure in `src/NetworkMonitoring.IntegrationConsole/Program.cs`
- [X] T046 [US3] Implement critical-flow objective evaluation service in `src/NetworkMonitoring.Backend/Application/Services/CriticalFlowObjectiveEvaluator.cs`
- [X] T047 [US3] Implement actionable alert payload builder in `src/NetworkMonitoring.Backend/Application/Models/OperationalAlertPayload.cs`
- [X] T048 [US3] Add alert trigger orchestration in `src/NetworkMonitoring.Backend/Host/Services/GraphRetentionHostedService.cs`
- [X] T068 [US3] Define and version critical-flow inventory for SC-003 coverage checks in `specs/008-observability-baseline/contracts/critical-flow-inventory.md`
- [X] T049 [US3] Add baseline alert rules and policy in `infrastructure/observability/alert-rules.yml`
- [X] T050 [US3] Wire observability CI gate into Jenkins pipeline in `Jenkinsfile`

**Checkpoint**: Proactive degradation detection and alerting are independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalize documentation, dashboards, and end-to-end verification.

- [X] T051 [P] Add baseline dashboards JSON bundle in `infrastructure/observability/grafana/dashboards/observability-baseline.json`
- [X] T052 [P] Update operational documentation for observability baseline in `infrastructure/documentation/README.md`
- [X] T053 [P] Update feature quickstart evidence section in `specs/008-observability-baseline/quickstart.md`
- [X] T054 Run full backend/probe/integration observability test suites and capture evidence in `specs/008-observability-baseline/quickstart.md`
- [X] T069 Validate SC-004 drill lead-time evidence (5-minute threshold against 5%-for-5m breach condition) in `specs/008-observability-baseline/quickstart.md`
- [X] T055 Validate constitution compliance and record result in `specs/008-observability-baseline/plan.md`
- [X] T060 Add explicit documentation compliance sweep for new public APIs/tests in `infrastructure/documentation/README.md`
- [X] T061 Add XML/TSDoc updates for all new public symbols introduced by this feature in `src/NetworkMonitoring.Backend/` and `src/NetworkMonitoring.Frontend/src/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Phase 1; blocks all user stories.
- **User Story phases (Phase 3-5)**: Depend on Phase 2 completion.
- **Polish (Phase 6)**: Depends on completion of required user stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on US2/US3.
- **US2 (P2)**: Starts after Foundational; can run after US1 or in parallel where files do not conflict.
- **US3 (P3)**: Starts after Foundational; depends on baseline signals from US1/US2 for realistic alerting validation.

### Within Each User Story

- Tests first, then implementation.
- Correlation/logging primitives before service-level instrumentation.
- Health/alert endpoints after baseline telemetry signals are available.

### Parallel Opportunities

- Phase 1 tasks marked `[P]` can run concurrently.
- Phase 2 tasks T008/T009/T010/T011/T014 can run in parallel after T007.
- US1 tests T016-T020, T057, and T065 can run in parallel.
- US2 tests T029-T031 can run in parallel.
- US3 tests T040-T042 and T067 can run in parallel.
- Polish tasks T051-T053 and T060 can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Run US1 observability contract tests in parallel
Task: "T016 CorrelationPropagationTests"
Task: "T017 StructuredLoggingContractTests"
Task: "T018 TelemetryHygieneTests"
Task: "T019 ProbeStructuredLoggingTests"
Task: "T020 IntegrationStructuredLoggingTests"
Task: "T057 diagnostics-correlation.test.ts"
Task: "T065 ElasticsearchLogCorrelationTests"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Implement Phase 3 (US1).
3. Validate diagnosis workflow and telemetry hygiene.
4. Demo MVP incident diagnosis capability.

### Incremental Delivery

1. Deliver US1 for correlation + structured diagnosis.
2. Deliver US2 for full distributed traceability + metrics baseline.
3. Deliver US3 for proactive degradation alerting and health readiness.
4. Finalize dashboards/docs/gates in Phase 6.

### Parallel Team Strategy

1. Team A: shared setup/foundational work.
2. Team B: US1 implementation/tests.
3. Team C: US2 implementation/tests after foundational completion.
4. Team D: US3 alerting/health implementation after baseline signals exist.

---

## Notes

- Foundation tasks provide baseline bootstrap wiring; user-story tasks complete scenario-specific
  behavior and evidence.
- `[P]` tasks are safe to run concurrently only when they do not touch the same files.
- Every user story phase has independent validation criteria aligned with `spec.md`.
- CI gate enforcement is mandatory for this feature because observability compliance is constitutional.
