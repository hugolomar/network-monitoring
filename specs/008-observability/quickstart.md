# Quickstart: Production Observability

## Goal

Validate that production-path services and platform components satisfy the observability contract for
correlation, structured logs, metrics, traces, health signaling, proactive alerting, pipeline business
metrics, and browser telemetry (ADR 0013 / FR-001..FR-019).

## Preconditions

- Local reference stack is running (`docker compose -f docker-compose.reference-stack.yml up -d`).
- Production-path services are running with observability enabled.
- Critical flows for validation are defined in `contracts/critical-flow-inventory.md`.

## 1) Validate correlation and traceability

1. Trigger one sampled critical operation that crosses multiple services.
2. Locate the operation by correlation identifier.
3. Confirm related logs, traces, and error context can be discovered from that identifier.
4. Confirm all participating service operations are linked under the same distributed trace lineage in
   Kibana APM.

Expected outcome: request/event is traceable end-to-end without server shell access.

## 2) Validate structured logging (application)

1. Trigger success and failure paths for sampled operations.
2. Confirm documents exist: `curl -sS "http://localhost:9200/observability-logs-*/_count"`.
3. In Kibana, query by `correlationId` and filter by `service.name` / `severity_text`.
4. Confirm no sensitive values appear in sampled records.

## 2b) Validate platform logs (Fluent Bit → Logstash)

1. Confirm platform components use the Forward logging driver and that `fluent-bit` / `logstash` are up.
2. Confirm platform indices receive documents:
   `curl -sS "http://localhost:9200/observability-logs-platform-*/_count"`.
3. Query a broker or store component by `service.name` (e.g. `kafka-1`, `postgres`).
4. Confirm multiline stack traces arrive as a single `body` and that unparsable lines carry
   `_platform_parse_failure` rather than disappearing.
5. Confirm Elasticsearch itself is **not** on this path; diagnose it with `docker logs` and metrics.

Expected outcome: SC-007 / SC-009 — platform logs are centralized and parse failures remain visible.

## 3) Validate log ↔ trace navigation (SC-008)

1. From a log document with `traceId`, follow the Grafana/Kibana data link into Kibana APM.
2. From an APM transaction, open related logs for the same `traceId` / `correlationId`.

Expected outcome: navigation completes in one UI family without re-running a manual search.

## 4) Validate service and platform metrics

1. Open Grafana dashboards under folder **Observability**:
   - `Observability Triage Overview`
   - `Service: Backend` / `Service: Probe` / `Service: Integration Console`
   - `Pipeline Stage Throughput`
2. Verify RED metrics for the backend, probe capture/domain counters, ingestion + lag, freshness, and
   container stats (`docker_stats` via Collector).

Expected outcome: baseline metric coverage plus pipeline stage rates (SC-011).

## 5) Validate capture-loss attribution (SC-012)

1. Induce or simulate capture drops (tshark drop lines) and unparsable input separately.
2. Confirm `capture_dropped_total` and `unparsable_input_total` move independently on the pipeline
   dashboard.

## 6) Validate end-to-end freshness (SC-013)

1. Ingest a device with a known `LastSeenUtc` in the past.
2. Confirm `observation_to_inventory_freshness_ms` records a positive latency on the backend dashboard.

## 7) Validate service health signaling

1. Query each service health status endpoint.
2. Confirm liveness and readiness semantics are machine-consumable.
3. Simulate one dependency issue and verify readiness impact behavior.

## 8) Validate alerting and Alertmanager (SC-004 / SC-010)

1. Confirm Prometheus loads `alert-rules.yml` and points at Alertmanager.
2. Simulate a controlled degradation against a defined critical flow.
3. Confirm Alertmanager delivers to the configured receiver (dev: `alert-sink`) and that warning
   alerts for the same `objective` are inhibited while critical is firing.
4. Capture timestamps for alert trigger and objective-breach confirmation to compute lead time.

## 9) Validate browser telemetry (SC-014 / FR-019)

1. Build/run the frontend with `VITE_OTEL_OTLP_HTTP_URL=http://localhost:4318`.
2. Perform a navigation and an API call; optionally force a network failure (backend stopped).
3. In Kibana APM, confirm service `network-monitoring-frontend` spans (`http.client`, `ui.navigation`,
   and/or client failure spans) and that backend spans share `traceparent` lineage for successful calls.
4. Confirm exported attributes omit end-user identity and URL query strings.

## 10) Validate objective gates

1. Run automated test suites covering observability obligations.
2. Run `./infrastructure/ci/check-observability.sh`.
3. Confirm failing obligations produce delivery failure in CI.

## Diagnostic lookup workflow (US1)

1. Capture `correlationId` from the failing backend response header `X-Correlation-ID`.
2. Open frontend **Diagnostics** page and enter `correlationId` (+ optional `traceId`).
3. Copy generated KQL and run it in Kibana Discover against `observability-logs-*`.
4. Filter by `service.name` and `severity_text`.
5. Follow `traceId` into Kibana APM when present.

## Kibana incident lookup guide

- Application logs: `correlationId : "<id>"` on `observability-logs-*`
- Platform logs: `service.name : "kafka-1"` on `observability-logs-platform-*`
- Optional: `severity_text : ("ERROR" or "WARN")`, `tags : "_platform_parse_failure"`

## SC-004 lead-time evidence template

- Drill run identifier:
- Critical flow:
- Alert triggered at UTC:
- Objective breach confirmed at UTC:
- Lead time (`breach - alert`):
- Meets >= 5 minutes requirement: yes/no

## Validation evidence (implementation)

### Automated suites (representative)

| Suite | Command | Result |
| --- | --- | --- |
| Probe unit (incl. capture-loss metrics) | `dotnet test tests/NetworkMonitoring.Probe.UnitTests` | Passed |
| Integration Console unit (incl. ingestion metrics) | `dotnet test tests/NetworkMonitoring.IntegrationConsole.UnitTests` | Passed |
| Backend unit (incl. intake + browser contract) | `dotnet test tests/NetworkMonitoring.Backend.UnitTests` | Passed |
| Frontend (incl. browser telemetry) | `npm --prefix src/NetworkMonitoring.Frontend test` | Passed |
| CI observability gate | `./infrastructure/ci/check-observability.sh` | PASS |

### Drill checklist (SC-007..SC-014)

| Criterion | Drill | Evidence location |
| --- | --- | --- |
| SC-007 Platform coverage | Platform log count + Grafana platform/container panels | §2b, §4 |
| SC-008 Log↔trace | Kibana log → APM | §3 |
| SC-009 Parse failure visibility | Tag `_platform_parse_failure` retained | §2b |
| SC-010 Alert delivery/suppression | Alertmanager → alert-sink; inhibition by `objective` | §8 |
| SC-011 Pipeline rates | Pipeline dashboard consecutive stages | §4 |
| SC-012 Loss cause | Independent capture vs unparsable series | §5 |
| SC-013 Freshness | `observation_to_inventory_freshness_ms` | §6 |
| SC-014 Browser segment | Frontend spans in APM | §9 |

Config-load note: Collector config validates to `Everything is ready` when started in isolation;
scrape errors to Kafka/Postgres/Elasticsearch are expected unless the full compose stack is up.
