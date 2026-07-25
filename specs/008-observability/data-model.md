# Data Model: Production Observability

## Overview

This feature defines a cross-cutting observability model for production-path runtime units and
platform components. The model describes required signal types, shared identity semantics, pipeline
stage measurements, and validation invariants used for objective verification.

## Entities

### 1) Correlation Context

Shared execution identity propagated across requests/events.

**Fields**
- `correlationId` (required)
- `traceId` (required for distributed flows)
- `parentSpanId` (optional when flow starts new trace)
- `originService` (required)

**Validation rules**
- Every critical operation carries `correlationId`.
- Cross-service operations preserve the same trace lineage, including browser → backend when present.
- Missing context on cross-boundary hops is a contract violation.

### 2) Structured Log Event

Machine-readable operational log emitted by authored services or normalized from platform components.

**Fields**
- `@timestamp` / event time (required)
- `service.name` (required)
- `service.type` (optional; `platform` for platform components)
- `severity_text` (required)
- `body` (required)
- `correlationId` (required for critical authored operations)
- `traceId` (required for distributed authored operations)
- `tags` (optional; includes `_platform_parse_failure` when normalization fails)
- `container.id` (optional; platform path)

**Validation rules**
- Required fields must be present after normalization.
- Prohibited sensitive content (PII, credentials, secrets) must not appear.
- Unparsed platform records are retained and tagged, not discarded.

### 3) Operational Metric Signal

Quantitative measurement for service and flow health.

**Fields**
- `metricName` (required)
- `metricType` (`counter`, `gauge`, `histogram`)
- `value` (required)
- `timestampUtc` (required)
- `labels` (service/environment/flow/outcome dimensions)

**Validation rules**
- Coverage includes availability, request volume, errors, response time, and relevant resource
  utilization.
- Metric dimensions must support cross-service filtering.
- Critical-flow success/failure metrics must be present.

### 4) Pipeline Stage

A named processing boundary in the monitoring pipeline whose throughput can be compared to the
previous and next stage.

**Fields**
- `stageId` (required; e.g. `capture`, `sessions_detected`, `devices_discovered`, `ingestion`,
  `inventory`, `graph_projection`)
- `throughputMetric` (required metric name)
- `lossMetrics` (optional; e.g. capture drop, unparsable, rejected ingest)

**Validation rules**
- Every stage publishes a rate so drop-off between consecutive stages is observable (SC-011).
- Capture saturation and parse failure remain distinguishable (SC-012).

### 5) Capture Loss Signal

Probe-side accounting of offered versus successfully mapped input.

**Fields**
- `packetsReceived` → `packets_received_total`
- `captureDropped` → `capture_dropped_total`
- `unparsableInput` → `unparsable_input_total`

**Validation rules**
- Capture drops and unparsable inputs MUST NOT share a single undifferentiated counter.

### 6) Distributed Trace Span

Execution segment in a distributed operation (including browser spans).

**Fields**
- `traceId` (required)
- `spanId` (required)
- `parentSpanId` (optional)
- `serviceName` (required)
- `operationName` (required)
- `startTimeUtc` (required)
- `endTimeUtc` (required)
- `status` (success/failure)

**Validation rules**
- Cross-service operations produce linked spans under one trace.
- Browser client failures that never reach a service still produce a client span when telemetry is
  enabled.
- Span attributes from the browser omit end-user identity and URL query/fragment.

### 7) Service Health Signal

Runtime liveness/readiness status for a deployable unit.

**Fields**
- `serviceName` (required)
- `statusType` (`live`, `ready`)
- `status` (`healthy`, `degraded`, `unhealthy`)
- `timestampUtc` (required)
- `details` (optional)

### 8) Operational Alert

Proactive notification raised when flow objectives are violated.

**Fields**
- `alertId` (required)
- `flowId` (required for critical flow)
- `severity` (required)
- `objective` (required for inhibition grouping)
- `triggeredAtUtc` (required)
- `correlationHints` (optional)
- `diagnosisContext` (required payload)

**Validation rules**
- Alert must trigger when a critical flow breaches agreed objectives.
- Warning alerts for an objective are inhibited while a critical alert for the same objective is active.

### 9) Critical Flow Definition

Business-relevant end-to-end path monitored by observability.

**Fields**
- `flowId` (required)
- `description` (required)
- `servicesInScope` (required)
- `operationalObjectives` (required)

## Relationships

- Correlation Context links Structured Log Event, Operational Metric Signal, Distributed Trace Span,
  and Operational Alert.
- Pipeline Stage sequences Operational Metric Signals for throughput comparison.
- Capture Loss Signal feeds the probe Pipeline Stage.
- Critical Flow Definition scopes required metrics, traces, and alert behavior.
- Service Health Signal is emitted per service and informs operational diagnosis context.

## State Transitions

### Distributed operation observability lifecycle

1. **Started**: Correlation context initialized (optionally in the browser) and first span/log emitted.
2. **In-flight**: Cross-service propagation creates linked spans/logs/metrics.
3. **Completed**: Operation marked success/failure; terminal signals emitted.
4. **Diagnosed**: If failure/degradation occurs, alert payload and log↔trace navigation provide entry
   context.

### Critical flow health lifecycle

1. **Healthy**: Objectives met, no active alert.
2. **Degrading**: Leading indicators deteriorate; warning alert may fire.
3. **Alerting**: Objective breach detected; critical alert emitted; warning inhibited.
4. **Recovered**: Flow returns to objective-compliant state; alert closes per policy.
