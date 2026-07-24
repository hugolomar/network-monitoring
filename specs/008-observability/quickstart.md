# Quickstart: Production Observability

## Goal

Validate that production-path services satisfy the observability baseline for correlation, structured
logs, metrics, traces, health signaling, and proactive alerting.

## Preconditions

- Local reference stack is running.
- Production-path services are running with baseline observability enabled.
- Critical flows for validation are defined.

## 1) Validate correlation and traceability

1. Trigger one sampled critical operation that crosses multiple services.
2. Locate the operation by correlation identifier.
3. Confirm related logs, traces, and error context can be discovered from that identifier.
4. Confirm all participating service operations are linked under the same distributed trace lineage.

Expected outcome:

- Request/event is traceable end-to-end without server shell access.

## 2) Validate structured logging baseline

1. Trigger success and failure paths for sampled operations.
2. Inspect log records from each participating service.
3. Confirm required fields exist (service, environment, severity, timestamp, correlation linkage).
4. Confirm no sensitive values appear in sampled records.

Expected outcome:

- Logs are machine-readable, correlated, and hygiene-compliant.

## 2b) Validate centralized log search (Elasticsearch/Kibana)

1. Ensure services are exporting logs via OTLP to collector (`OTEL_EXPORTER_OTLP_ENDPOINT` / `Observability:OtlpEndpoint`).
2. Confirm documents exist: `curl -sS "http://localhost:9200/observability-logs-*/_count"` (count > 0).
3. Open Kibana and query logs for a sampled failing operation using `correlationId`.
4. Filter by service and severity to confirm cross-service traceability of related events.
5. Save the KQL query and screenshot evidence for incident runbook attachment.

Expected outcome:

- Operators can retrieve correlated cross-service logs via Kibana without server shell access.

## 3) Validate service metrics baseline

1. Generate representative load for one critical flow.
2. Verify metrics exist for availability, volume, errors, timing, and relevant resources.
3. Verify critical-flow success/failure metrics are present.

Expected outcome:

- Baseline metric coverage exists across all in-scope services.

## 4) Validate service health signaling

1. Query each service health status endpoint.
2. Confirm liveness and readiness semantics are machine-consumable.
3. Simulate one dependency issue and verify readiness impact behavior.

Expected outcome:

- Operators can determine if each service is alive and ready for traffic.

## 5) Validate alerting behavior

1. Simulate a controlled degradation against a defined critical flow.
2. Confirm alert generation when objectives are breached.
3. Confirm alert payload includes: `flowId`, breach reason, timestamp, impacted component, severity,
   and correlation/trace linkage (when available).
4. Capture timestamps for alert trigger and objective-breach confirmation to compute lead time.

Expected outcome:

- Alert arrives before severe user impact threshold in validation scenarios.

## 6) Validate objective gates

1. Run automated test suites covering observability baseline obligations.
2. Run CI checks enforcing mandatory observability gates.
3. Confirm failing baseline obligations produce delivery failure.

Expected outcome:

- Observability baseline is objectively verifiable and gate-enforced.

## 7) Validate frontend diagnostics surface

1. Trigger one failing flow and open the frontend diagnostic/operation view.
2. Confirm the frontend can search or link by correlation identifier.
3. Confirm distributed trace linkage and impacted component context are visible to operators.
4. Confirm no sensitive values are rendered in diagnostic payload fragments.

Expected outcome:

- Frontend participation in diagnostics aligns with cross-cutting scope and does not require server access.

## Suggested validation evidence

- Test run outputs for correlation propagation and distributed trace completeness.
- Sample structured log payloads demonstrating required fields and hygiene compliance.
- Metric snapshots showing baseline coverage by service and critical flow.
- Health endpoint responses under normal and degraded conditions.
- Alert payload sample from controlled degradation drill.
- CI gate result proving fail-on-noncompliance behavior.

## Diagnostic lookup workflow (US1)

1. Capture `correlationId` from the failing backend response header `X-Correlation-ID`.
2. Open frontend **Diagnostics** page and enter `correlationId` (+ optional `traceId`).
3. Copy generated KQL query and run it in Kibana Discover against `observability-logs-*`.
4. Filter by `service.name` and `severity_text` to isolate backend/probe/integration-console events.
5. Confirm the same `correlationId` appears across all involved services.

## Kibana incident lookup guide

- Baseline KQL query:
  - `correlationId : "<id>"`
- Optional narrowing:
  - `service.name : "network-monitoring-backend"`
  - `severity_text : ("ERROR" or "WARN")`
- Expected fields:
  - `@timestamp`, `service.name`, `severity_text`, `correlationId`, `traceId`, `body`

## SC-004 lead-time evidence template

- Drill run identifier:
- Critical flow:
- Alert triggered at UTC:
- Objective breach confirmed at UTC:
- Lead time (`breach - alert`):
- Meets >= 5 minutes requirement: yes/no

## Validation evidence (latest run)

- Backend unit tests:
  - `dotnet test tests/NetworkMonitoring.Backend.UnitTests/NetworkMonitoring.Backend.UnitTests.csproj`
  - Result: **Passed** (20/20)
- Backend integration tests:
  - `dotnet test tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj`
  - Result: **Passed** (34/34)
- Probe unit tests:
  - `dotnet test tests/NetworkMonitoring.Probe.UnitTests/NetworkMonitoring.Probe.UnitTests.csproj`
  - Result: **Passed** (47/47)
- Integration Console unit tests:
  - `dotnet test tests/NetworkMonitoring.IntegrationConsole.UnitTests/NetworkMonitoring.IntegrationConsole.UnitTests.csproj`
  - Result: **Passed** (22/22)
- Frontend tests:
  - `npm --prefix "/home/hugo/network-monitoring/src/NetworkMonitoring.Frontend" test`
  - Result: **Passed** (13/13)

Observed notes:

- NuGet emitted vulnerability advisories for current dependency versions during test restore.
- These advisories do not block the baseline checks in this implementation slice but should be tracked in dependency hardening work.
