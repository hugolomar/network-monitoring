# Research: Production Observability

## Decision 1: Definitive stack alignment

- **Decision**: Adopt the stack defined in ADR 0013 as the reference implementation path for this
  feature (OpenTelemetry SDK + Collector for application telemetry and platform metrics; Prometheus,
  Alertmanager, Grafana; Elastic APM Server + Elasticsearch + Kibana for traces and logs; Fluent Bit +
  Logstash for platform logs; browser OTLP/HTTP to the Collector).
- **Rationale**: ADR 0013 closes the platform-coverage, alert-delivery, and log-to-trace gaps that the
  earlier baseline left open, and records the measured choice of push-based platform log collection
  over reading the runtime's private log files.
- **Alternatives considered**:
  - Re-open stack selection inside the feature spec (rejected: duplicates ADR scope).
  - Retain Jaeger beside Elastic APM (rejected: two permanent trace backends).
  - Collector `filelog` / `fluentforward` on the platform log path (rejected: multiline and model
    mismatch; see ADR 0013 validation).

## Decision 2: Cross-cutting capability boundary

- **Decision**: Treat observability as a cross-cutting capability applied to each production-path
  deployable unit (Probe, Integration Console, Backend, Frontend) rather than one centralized module.
- **Rationale**: Architecture SSOT requires deployable independence and participant-local diagnostics.
- **Alternatives considered**:
  - Centralized gateway-only instrumentation (rejected: loses service-local diagnostic clarity).
  - Backend-only observability rollout (rejected: violates cross-cutting feature intent).

## Decision 3: Signal taxonomy

- **Decision**: Required signals are correlation context, structured logs, service metrics, distributed
  traces, health signals, actionable alerts, platform logs/metrics, pipeline business metrics, and
  browser telemetry for user-visible failures and timings.
- **Rationale**: Satisfies constitutional Articles 32-36 and FR-001..FR-019.
- **Alternatives considered**:
  - Logs + metrics only (rejected: weak distributed failure diagnosis).
  - Vendor RUM agent in the browser (rejected: second instrumentation standard; see ADR 0013).

## Decision 4: Correlation propagation scope

- **Decision**: Require propagation of correlation and trace context across synchronous and
  asynchronous boundaries, including browser → backend HTTP.
- **Rationale**: The platform is event-driven and user operations start in the browser.
- **Alternatives considered**:
  - HTTP-only server propagation (rejected: incomplete for stream-driven paths and browser starts).

## Decision 5: Telemetry data hygiene

- **Decision**: Explicit no-PII/no-secret obligations with testable redaction on application paths and
  on the platform Logstash path (platform logs do not pass the Collector).
- **Rationale**: Security-by-default and usable incident search both require predictable hygiene.
- **Alternatives considered**:
  - Developer convention only (rejected: high leakage risk).

## Decision 6: Health and alerting semantics

- **Decision**: Machine-consumable health plus Prometheus rules delivered through Alertmanager with
  grouping, inhibition by shared `objective` label, and maintenance windows.
- **Rationale**: Operators need proactive detection with controlled noise.
- **Alternatives considered**:
  - Rules without Alertmanager (rejected: constitutional delivery obligation unmet).

## Decision 7: Objective verification strategy

- **Decision**: Enforce observability through automated verification gates (tests + CI checks) instead
  of documentation-only acceptance.
- **Rationale**: Constitutional Article 36 requires delivery to fail when mandatory obligations are
  unmet. Gate script: `infrastructure/ci/check-observability.sh`.

## Decision 8: Platform log path topology

- **Decision**: Platform container standard output → Docker Forward logging driver (async) → Fluent Bit
  (multiline assembly) → Logstash (grok, field contract, redaction, PQ/DLQ) → Elasticsearch. Application
  logs remain SDK → Collector → Elasticsearch.
- **Rationale**: Measured multiline assembly and flat record shape; no host log directory or runtime
  control socket for log collection. Elasticsearch is excluded from this path to avoid self-ingest.
- **Measured payload shape (Fluent Bit → Logstash HTTP JSON)**: flat objects with `container_name`,
  `container_id`, `source`, `log`. After Logstash: `service.name`, `service.type`, `severity_text`,
  `@timestamp`, `body`, optional `tags` for parse failures.
- **Alternatives considered**: documented in ADR 0013.

## Decision 9: Platform and container metrics

- **Decision**: Collector scrapes Kafka, PostgreSQL, Elasticsearch, HTTP checks for Connect/Neo4j, and
  `docker_stats` for per-container CPU/memory; exposes them on the Prometheus exporter.
- **Rationale**: Platform health must be visible without SDK instrumentation. The runtime socket is
  accepted for stats only, not for log collection.

## Decision 10: Business-flow metric taxonomy

- **Decision**: Emit stage counters/gauges/histograms at Probe, Integration Console, and Backend so
  consecutive stages are comparable and capture saturation can be distinguished from parse failure.
- **Names**: `packets_received_total`, `capture_dropped_total`, `unparsable_input_total`,
  `sessions_detected_total`, `devices_discovered_total`, `devices_ingested_total`,
  `kafka_consumer_lag`, `observation_to_inventory_freshness_ms`.

## Per-hop responsibilities (platform logs)

| Hop | Responsibility |
| --- | --- |
| Runtime logging driver | Push stdout/stderr; async buffer; attach container metadata |
| Fluent Bit | Receive Forward protocol; assemble multiline by log layout; durable filesystem buffer; HTTP JSON to Logstash |
| Logstash | Grok by component; promote event time; map severity; redact; strip transport fields; PQ + DLQ; index |
| Elasticsearch | Store and search; shared with APM for log↔trace navigation |
