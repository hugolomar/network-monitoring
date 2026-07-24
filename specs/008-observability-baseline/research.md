# Research: Production Observability Baseline

## Decision 1: Baseline stack alignment

- **Decision**: Adopt the stack defined in ADR 0012 as the reference implementation path for this
  feature slice (OpenTelemetry, OpenTelemetry Collector, Prometheus, Grafana, Jaeger, Elasticsearch,
  Kibana, structured JSON logging).
- **Rationale**: The ADR already resolves stack-level tradeoffs and provides a vendor-neutral
  instrumentation baseline compatible with the current platform.
- **Alternatives considered**:
  - Re-open stack selection inside the feature spec (rejected: duplicates ADR scope and delays delivery).
  - Define only abstract requirements with no reference runtime path (rejected: weak implementation
    convergence and verification risk).

## Decision 2: Cross-cutting capability boundary

- **Decision**: Treat observability baseline as a cross-cutting capability applied to each
  production-path deployable unit (Probe, Integration Console, Backend, Frontend where applicable)
  rather than one centralized module.
- **Rationale**: Architecture SSOT requires deployable independence and participant-local diagnostics;
  centralized-only telemetry ownership would blur responsibilities.
- **Alternatives considered**:
  - Centralized gateway-only instrumentation (rejected: loses service-local diagnostic clarity).
  - Backend-only observability rollout (rejected: violates cross-cutting feature intent).

## Decision 3: Signal taxonomy for baseline verification

- **Decision**: Baseline must include: correlation context, structured logs, service metrics,
  distributed traces, health signals, and actionable alerts.
- **Rationale**: This directly satisfies constitutional Articles 32-36 and feature FR-001..FR-010.
- **Alternatives considered**:
  - Logs + metrics only (rejected: weak distributed failure diagnosis).
  - Traces only (rejected: insufficient aggregate operational health visibility).

## Decision 4: Correlation propagation scope

- **Decision**: Require propagation of correlation and trace context across both synchronous and
  asynchronous boundaries.
- **Rationale**: The platform is event-driven; excluding async boundaries would break end-to-end flow
  diagnosis.
- **Alternatives considered**:
  - HTTP-only propagation (rejected: incomplete traceability for stream-driven paths).
  - Service-local correlation IDs with no propagation (rejected: cannot reconstruct distributed flow).

## Decision 5: Telemetry data hygiene enforcement

- **Decision**: Define explicit no-PII/no-secret telemetry obligations and require testable verification
  of redaction/masking behavior.
- **Rationale**: Security-by-default obligations and observability usability both require predictable
  hygiene controls.
- **Alternatives considered**:
  - Rely on developer convention only (rejected: high leakage risk).
  - Manual log review only (rejected: non-scalable and non-objective).

## Decision 6: Health and alerting baseline semantics

- **Decision**: Require machine-consumable service health status and alert triggers tied to agreed
  critical-flow operational objectives.
- **Rationale**: Operators need proactive detection, not only reactive diagnosis after user impact.
- **Alternatives considered**:
  - Health endpoints without alerts (rejected: late detection).
  - Alerts without explicit health/readiness semantics (rejected: ambiguous remediation entry point).

## Decision 7: Objective verification strategy

- **Decision**: Enforce observability baseline through automated verification gates (tests + CI checks)
  instead of documentation-only acceptance.
- **Rationale**: Constitutional Article 36 requires delivery to fail when mandatory observability
  obligations are unmet.
- **Alternatives considered**:
  - Manual validation checklists only (rejected: weak regression protection).
  - Post-deployment-only verification (rejected: delayed feedback and higher risk).
