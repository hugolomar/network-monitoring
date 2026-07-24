# Data Model: Production Observability Baseline

## Overview

This feature defines a cross-cutting observability model for production-path runtime units. The model
describes required signal types, shared identity semantics, and validation invariants used for
objective verification.

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
- Cross-service operations preserve the same trace lineage.
- Missing context on cross-boundary hops is a baseline violation.

### 2) Structured Log Event

Machine-readable operational log emitted by services.

**Fields**
- `timestampUtc` (required)
- `serviceName` (required)
- `environment` (required)
- `severity` (required)
- `message` (required)
- `correlationId` (required for critical operations)
- `traceId` (required for distributed operations)
- `errorCode` (optional)

**Validation rules**
- Required fields must be present.
- Prohibited sensitive content (PII, credentials, secrets) must not appear.
- Severity must map to an accepted level taxonomy.

### 3) Operational Metric Signal

Quantitative measurement for service and flow health.

**Fields**
- `metricName` (required)
- `metricType` (`counter`, `gauge`, `histogram`)
- `value` (required)
- `timestampUtc` (required)
- `labels` (service/environment/flow dimensions)

**Validation rules**
- Baseline coverage includes availability, request volume, errors, response time, and relevant
  resource utilization.
- Metric dimensions must support cross-service filtering.
- Critical-flow success/failure metrics must be present.

### 4) Distributed Trace Span

Execution segment in a distributed operation.

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
- Span timing must support wait/error localization.
- Missing participant spans for critical flow indicate incomplete trace coverage.

### 5) Service Health Signal

Runtime liveness/readiness status for a deployable unit.

**Fields**
- `serviceName` (required)
- `statusType` (`live`, `ready`)
- `status` (`healthy`, `degraded`, `unhealthy`)
- `timestampUtc` (required)
- `details` (optional)

**Validation rules**
- Each production-path service exposes machine-consumable health state.
- Readiness semantics must indicate traffic handling capability.

### 6) Operational Alert

Proactive notification raised when flow objectives are violated.

**Fields**
- `alertId` (required)
- `flowId` (required for critical flow)
- `severity` (required)
- `triggeredAtUtc` (required)
- `correlationHints` (optional)
- `diagnosisContext` (required baseline payload)

**Validation rules**
- Alert must trigger when a critical flow breaches agreed objectives.
- Payload must identify affected flow and provide diagnosis start context.
- Duplicate behavior across prolonged incidents must be deterministically handled.

### 7) Critical Flow Definition

Business-relevant end-to-end path monitored by baseline observability.

**Fields**
- `flowId` (required)
- `description` (required)
- `servicesInScope` (required)
- `operationalObjectives` (required)

**Validation rules**
- Every defined critical flow has success/failure telemetry coverage.
- Objectives map to alert conditions and verification criteria.

## Relationships

- Correlation Context links Structured Log Event, Operational Metric Signal, Distributed Trace Span, and
  Operational Alert.
- Critical Flow Definition scopes required metrics, traces, and alert behavior.
- Service Health Signal is emitted per service and informs operational diagnosis context.
- Operational Alert references one impacted Critical Flow and associated diagnostic context.

## State Transitions

### Distributed operation observability lifecycle

1. **Started**: Correlation context initialized and first span/log emitted.
2. **In-flight**: Cross-service propagation creates linked spans/logs/metrics.
3. **Completed**: Operation marked success/failure; terminal signals emitted.
4. **Diagnosed**: If failure/degradation occurs, alert payload provides diagnosis entry context.

### Critical flow health lifecycle

1. **Healthy**: Objectives met, no active alert.
2. **Degrading**: Leading indicators deteriorate; alert conditions approaching thresholds.
3. **Alerting**: Objective breach detected; alert emitted with context.
4. **Recovered**: Flow returns to objective-compliant state; alert closes per policy.
