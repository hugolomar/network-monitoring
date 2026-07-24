# ADR 0012: Observability Stack Baseline

- Status: Superseded by ADR 0013
- Date: 2026-07-07
- Superseded by: [ADR 0013](0013-definitive-observability-stack.md) (definitive observability stack)

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

- **Instrumentation standard:** OpenTelemetry for logs, metrics, and traces in services.
- **Telemetry transport/control plane:** OpenTelemetry Collector (OTLP ingest, pipeline routing,
  exporter decoupling).
- **Metrics storage/query:** Prometheus.
- **Dashboards and visualization:** Grafana.
- **Trace backend and UI:** Jaeger (via OpenTelemetry Collector export path).
- **Structured application logs:** JSON console logs plus OpenTelemetry log export with mandatory
  correlation/trace enrichment.
- **Centralized log storage/search:** Elasticsearch indices.
- **Log exploration UI:** Kibana.

Baseline obligations by runtime path:

- Critical flows MUST emit structured logs, counters/histograms, and distributed trace spans.
- Cross-boundary calls/events MUST propagate correlation and trace context.
- Dashboards and alert rules MUST be derived from the stack above and tied to agreed SLI/SLO
  signals.

This ADR defines the stack choice. Thresholds, concrete signal names, and rollout sequencing remain
specified in feature specs/plans (starting with `008-observability`).

## Rationale

- **Open standard and low lock-in:** OpenTelemetry + OTLP keeps instrumentation portable.
- **.NET ecosystem fit:** Native support in .NET 10 hosting and instrumentation libraries.
- **Separation of concerns:** Collector decouples app code from backend-specific exporters and allows
  routing/pipeline changes without service-code churn.
- **Metrics/traces fit:** Prometheus/Grafana/Jaeger cover SLI/SLO dashboards, alert expressions, and
  distributed trace diagnosis with mature operational patterns.
- **Log diagnosis requirements fit:** Elasticsearch + Kibana support advanced indexed search and
  high-selectivity filtering over structured fields (`correlationId`, `traceId`, `service`, `severity`)
  required for cross-service incident analysis.
- **Existing stack reuse:** Elasticsearch is already part of the platform runtime, reducing adoption cost
  while preserving required search depth for logs.
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

6. **Loki for centralized logs**
   - Pros: lightweight operation and tight Grafana integration.
   - Cons: weaker fit for this baseline's indexed-search-first diagnostics requirements compared with
     Elasticsearch/Kibana (rich field filtering and exploratory investigation workflows).
   - Rejected in favor of Elasticsearch + Kibana for baseline log centralization/search.

## Consequences

- **Positive:** establishes one consistent telemetry architecture across services and environments;
  enables verifiable observability gates and baseline dashboards/alerts.
- **Negative:** introduces additional operational components (Collector, Prometheus, Grafana, Jaeger,
  Kibana) and indexed log governance concerns (retention/mapping) that must be configured and
  maintained.
- **Implementation note:** existing no-op telemetry adapters (for example graph telemetry) should be
  replaced by OpenTelemetry-backed implementations while preserving clean/hexagonal boundaries.
- **Governance note:** future telemetry backend substitutions remain possible if they preserve OTel
  instrumentation contracts and constitutional observability obligations.
