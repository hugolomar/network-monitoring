# Contract: Production Observability

## Purpose

Define the cross-service observability behavior contract for production-path services and platform
components, including mandatory telemetry signals, propagation requirements, health signaling,
alerting expectations, pipeline business metrics, and browser telemetry.

## In-Scope Runtime Units

- Probe runtime unit
- Integration Console runtime unit
- Backend runtime unit
- Frontend runtime unit (browser emitter and diagnostics surface)
- Platform components in the reference stack: message broker, connector runtime, relational store,
  graph store (logs and/or health metrics). The search store is observed through metrics; its own
  logs are not centralized into itself (see ADR 0013 and note 007).

## Signal Requirements

### Correlation

- Every critical request/event MUST include a shared correlation identifier.
- Distributed execution MUST preserve trace context across synchronous and asynchronous boundaries.
- Browser-originated API calls MUST propagate W3C `traceparent` so the user operation and backend
  spans share one trace.

### Structured Logging

Each authored production-path service MUST emit structured logs with:

- service identity,
- environment identity,
- severity level,
- timestamp,
- correlation linkage for critical operations,
- error context for failures.

Platform component logs MUST be normalized to the same field contract for:

- `service.name`,
- `severity_text`,
- `@timestamp` (event time from the body when parseable),
- `body`,
- `service.type` (`platform` for platform components).

Records that cannot be normalized MUST be retained and tagged as parse failures rather than dropped.

Prohibited content in logs and telemetry:

- personal data,
- credentials,
- secrets,
- raw sensitive payload material.

Centralized log behavior:

- Application logs: SDK → Collector → Elasticsearch (`observability-logs-*`).
- Platform logs: runtime logging driver → Fluent Bit → Logstash → Elasticsearch
  (`observability-logs-platform-*`).
- Operators MUST be able to search by `correlationId`, `traceId`, service, and severity in Kibana,
  and navigate from a log line to its APM trace when `traceId` is present.

### Metrics

Each production-path service MUST publish metrics for:

- availability,
- request/operation volume,
- error volume,
- response/operation timing,
- relevant resource utilization.

Pipeline business metrics MUST include at minimum:

| Stage | Metric names |
| --- | --- |
| Probe capture | `packets_received_total`, `capture_dropped_total`, `unparsable_input_total` |
| Probe domain | `sessions_detected_total`, `devices_discovered_total` |
| Integration Console | `devices_ingested_total` (by `outcome`), `kafka_consumer_lag` |
| Backend intake | `observation_to_inventory_freshness_ms` |
| Backend graph | `graph_projection_total`, `graph_projection_latency_ms` |
| Platform / search store | Collector receivers including Elasticsearch indexing rejections and `docker_stats` |

Critical business flows MUST expose success and failure signal coverage.

### Distributed Tracing

- Distributed operations MUST generate complete traces across participating dependencies.
- Trace data is stored via Elastic APM Server in Elasticsearch and explored in Kibana APM.
- Browser segments (`http.client`, `document.load`, `ui.navigation`, client failure spans) MUST be
  present for sampled user operations when browser export is enabled.

### Service Health

- Each service MUST expose machine-consumable health status for runtime liveness/readiness.
- Readiness status MUST indicate whether the service is prepared to receive traffic.

### Alerts

- Alerts MUST be generated when critical flows fail agreed operational objectives.
- Prometheus loads alert rules and delivers them through Alertmanager with grouping and inhibition.
- Alert payloads MUST identify the affected flow and include diagnosis-start context.
- Minimum required alert payload fields:
  - `flowId`
  - `breachReason`
  - `triggeredAtUtc`
  - `impactedComponent`
  - `severity`
  - `correlationId` or `traceId` (when available)

### Browser Telemetry Hygiene

Browser-exported attributes MUST NOT include end-user identity. URL attributes MUST omit query and
fragment. Forbidden keys include `enduser.id`, `user.email`, and
`http.request.header.authorization` (see Frontend `telemetry/hygiene.ts` and Backend
`BrowserTelemetryContractTests`).

## Validation Contract

The capability is considered compliant only when verified by automated checks:

- tests validating mandatory telemetry presence and propagation behavior,
- checks validating no-sensitive-data telemetry hygiene (including browser payload contract),
- checks validating health exposure and alert triggering behavior under degradation scenarios,
- CI gate behavior that fails delivery when mandatory obligations are unmet
  (`infrastructure/ci/check-observability.sh`).

## Compatibility Rules

- The contract is additive and cross-cutting; it MUST NOT redefine domain authority boundaries.
- Service-specific extensions may add fields/signals but MUST preserve contract semantics.
- Tool/backend substitutions are permitted only if behavior and verification obligations remain
  equivalent (ADR supersession required for reference-stack changes).
