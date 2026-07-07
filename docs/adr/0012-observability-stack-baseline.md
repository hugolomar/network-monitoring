# ADR 0012: Observability Stack Baseline

- Status: Accepted
- Date: 2026-07-07

## Context

The platform currently has partial observability signals and governance requirements that now
mandate verifiable operational telemetry for production-path services (Probe, Integration Console,
Backend, and related runtime boundaries).

Current gaps include:

- inconsistent structured logging across services,
- no unified telemetry SDK/export pipeline,
- no standard metrics backend/dashboard baseline,
- no distributed trace visualization backend,
- no objective operational baseline that can be validated in CI and local reference environments.

The project needs one concrete, vendor-neutral observability stack that is compatible with the
existing .NET 10 services, Docker-based local/reference stack, and incremental feature delivery.

## Decision

Adopt the following observability stack baseline:

- **Instrumentation standard:** OpenTelemetry for metrics and traces in services.
- **Telemetry transport/control plane:** OpenTelemetry Collector (OTLP ingest, pipeline routing,
  exporter decoupling).
- **Metrics storage/query:** Prometheus.
- **Dashboards and visualization:** Grafana.
- **Trace backend and UI:** Jaeger (via OpenTelemetry Collector export path).
- **Structured application logs:** Serilog JSON output with mandatory correlation/trace enrichment.

Baseline obligations by runtime path:

- Critical flows MUST emit structured logs, counters/histograms, and distributed trace spans.
- Cross-boundary calls/events MUST propagate correlation and trace context.
- Dashboards and alert rules MUST be derived from the stack above and tied to agreed SLI/SLO
  signals.

This ADR defines the stack choice. Thresholds, concrete signal names, and rollout sequencing remain
specified in feature specs/plans (starting with `observability-baseline`).

## Rationale

- **Open standard and low lock-in:** OpenTelemetry + OTLP keeps instrumentation portable.
- **.NET ecosystem fit:** Native support in .NET 10 hosting and instrumentation libraries.
- **Separation of concerns:** Collector decouples app code from backend-specific exporters.
- **Operational familiarity:** Prometheus/Grafana/Jaeger are established observability components.
- **Incremental adoption:** Allows phased rollout (health/logging first, then metrics/traces) without
  redesigning the stack.

## Alternatives Considered

1. **Single-vendor APM suite only (Datadog/New Relic/Dynatrace)**
   - Pros: strong out-of-the-box experience, unified UI.
   - Cons: higher lock-in and cost coupling; less alignment with current local-first reference stack.
   - Rejected as baseline for this repository.

2. **Prometheus + Grafana without OpenTelemetry**
   - Pros: simpler initial metrics setup.
   - Cons: weaker cross-signal standardization; trace strategy becomes fragmented/vendor-specific.
   - Rejected.

3. **OpenTelemetry direct export from services (no Collector)**
   - Pros: fewer components.
   - Cons: tighter coupling between services and telemetry backends; harder routing/sampling changes.
   - Rejected for baseline.

4. **Logs-only baseline**
   - Pros: minimal implementation effort.
   - Cons: does not satisfy mandatory metrics/traces requirements and weakens objective verification.
   - Rejected.

5. **Elastic-only observability stack for all signals**
   - Pros: single ecosystem for logs/metrics/traces.
   - Cons: diverges from selected Prometheus/Grafana operational model and duplicates existing search
     projection concerns.
   - Rejected for this baseline.

## Consequences

- **Positive:** establishes one consistent telemetry architecture across services and environments;
  enables verifiable observability gates and baseline dashboards/alerts.
- **Negative:** introduces additional operational components (Collector, Prometheus, Grafana, Jaeger)
  that must be configured, secured, and maintained.
- **Implementation note:** existing no-op telemetry adapters (for example graph telemetry) should be
  replaced by OpenTelemetry-backed implementations while preserving clean/hexagonal boundaries.
- **Governance note:** future telemetry backend substitutions remain possible if they preserve OTel
  instrumentation contracts and constitutional observability obligations.
