# Contract: Production Observability

## Purpose

Define the cross-service observability behavior contract for production-path services, including
mandatory telemetry signals, propagation requirements, health signaling, and alerting expectations.

## In-Scope Runtime Units

- Probe runtime unit
- Integration Console runtime unit
- Backend runtime unit
- Frontend runtime unit and application diagnostics surface

## Baseline Signal Requirements

### Correlation

- Every critical request/event MUST include a shared correlation identifier.
- Distributed execution MUST preserve trace context across synchronous and asynchronous boundaries.

### Structured Logging

Each production-path service MUST emit structured logs with:

- service identity,
- environment identity,
- severity level,
- timestamp,
- correlation linkage for critical operations,
- error context for failures.

Prohibited content in logs:

- personal data,
- credentials,
- secrets,
- raw sensitive payload material.

Centralized log behavior:

- Structured logs MUST be exported to Elasticsearch using a stable baseline index pattern.
- Operators MUST be able to search by `correlationId`, `traceId`, service, and severity in Kibana.
- Log search behavior MUST support cross-service diagnosis without direct server shell access.

### Metrics

Each production-path service MUST publish baseline metrics for:

- availability,
- request/operation volume,
- error volume,
- response/operation timing,
- relevant resource utilization.

Critical business flows MUST expose success and failure signal coverage.

### Distributed Tracing

- Distributed operations MUST generate complete traces across participating dependencies.
- Trace data MUST enable identification of dependencies, wait points, and failure locations.

### Service Health

- Each service MUST expose machine-consumable health status for runtime liveness/readiness.
- Readiness status MUST indicate whether the service is prepared to receive traffic.

### Alerts

- Alerts MUST be generated when critical flows fail agreed operational objectives.
- Alert payloads MUST identify the affected flow and include diagnosis-start context.
- Minimum required alert payload fields:
  - `flowId`
  - `breachReason`
  - `triggeredAtUtc`
  - `impactedComponent`
  - `severity`
  - `correlationId` or `traceId` (when available)

## Validation Contract

The baseline is considered compliant only when verified by automated checks:

- tests validating mandatory telemetry presence and propagation behavior,
- checks validating no-sensitive-data telemetry hygiene,
- checks validating health exposure and alert triggering behavior under degradation scenarios,
- CI gate behavior that fails delivery when mandatory baseline obligations are unmet.

## Compatibility Rules

- The contract is additive and cross-cutting; it MUST NOT redefine domain authority boundaries.
- Service-specific extensions may add fields/signals but MUST preserve baseline semantics.
- Tool/backend substitutions are permitted only if baseline behavior and verification obligations remain
  equivalent.
