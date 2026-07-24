# ADR 0013: Definitive Observability Stack

- Status: Accepted
- Date: 2026-07-24
- Supersedes: [ADR 0012](0012-observability-stack-baseline.md) (observability stack baseline)

## Context

[ADR 0012](0012-observability-stack-baseline.md) established the first observability stack and was
implemented in feature `008-observability`. Operating that baseline against the reference
stack exposed four gaps that the baseline decision did not cover:

- **Platform logs are not collected at all.** The Collector only exposes OTLP receivers, so logs from
  Kafka, Kafka Connect, Postgres, and Neo4j exist solely in `docker logs`.
- **Platform metrics are not collected either.** Prometheus scrapes only the Collector, so the health
  of Kafka, Postgres, Neo4j, and Elasticsearch is invisible.
- **Alerts cannot reach anyone.** `alert-rules.yml` is not referenced from `prometheus.yml` and there
  is no Alertmanager, which conflicts with the constitutional obligation to alert on SLO breaches.
- **Traces and logs live in separate stores.** Traces are in Jaeger and logs in Elasticsearch, so an
  operator cannot navigate from a log line to the trace that produced it.

The baseline therefore delivered observability of the services the team writes, but not of the
platform they run on, and not the cross-signal navigation that incident diagnosis depends on.

This ADR records the stack that closes those gaps. The wiring choices below were validated
experimentally before being written down; see [Validation](#validation).

## Decision

Adopt the following observability stack for all runtime paths.

### Instrumentation and telemetry control plane

- **Instrumentation standard:** OpenTelemetry SDK in Backend, Probe, Integration Console, and
  Frontend, for logs, metrics, and traces. Unchanged from the baseline.
- **Browser telemetry:** the OpenTelemetry browser SDK, exporting OTLP over HTTP to the Collector.
  A vendor RUM agent is deliberately not used, so the "one standard inside the code" rule holds for
  the browser as well. The browser is an emitter of telemetry, not only a consumer of it.
- **Telemetry control plane:** OpenTelemetry Collector remains the single ingest point, with an
  expanded role. In addition to receiving OTLP from services, it now:
  - tails platform container log files (`filelog` receiver),
  - collects platform metrics through component-specific receivers (Kafka, Postgres,
    Elasticsearch, container runtime).

### Metrics

- **Metrics storage and query:** Prometheus. Unchanged from the baseline.
- **Alert routing:** **Alertmanager** (new), fed by Prometheus rule evaluation.
- **Dashboards:** Grafana. Unchanged from the baseline.

### Traces

- **Trace backend and UI:** **Elastic APM Server** (new), receiving OTLP from the Collector and
  storing into Elasticsearch.
- **Jaeger is removed.** It is not retained alongside Elastic APM.

### Logs

- **Application log path:** SDK → Collector → Elasticsearch.
- **Platform log path:** Collector (`filelog`) → OTLP/JSON → **Fluent Bit** (new) → HTTP/JSON →
  **Logstash** (new) → Elasticsearch.
- **Fluent Bit role:** OTLP-to-flat-JSON adapter only. It does **not** tail files and does **not**
  handle metrics.
- **Logstash role:** normalization point for platform logs (grok parsing, field contract, index
  routing), with persistent queue and dead letter queue enabled.
- **Centralized storage and search:** Elasticsearch, which also stores APM data.
- **Exploration UI:** Kibana, which also serves as the APM UI and provides log-to-trace navigation.

### Standard output policy

Services keep writing to standard output as a fallback channel that does not depend on the telemetry
pipeline, but the two sinks are tuned differently:

- OTLP export carries `Information` and above, because it lands in a searchable store.
- Console output carries `Warning` and above, because its only consumers are `docker logs` and
  startup or crash diagnosis.

Nothing tails application container standard output, so application logs are never indexed twice.

## Rationale

### What each component contributes

- **OpenTelemetry SDK.** The only layer that knows which trace is active while a line of code runs,
  and therefore the only one that can attach `traceId` to a log record. Also the source of runtime
  metrics and hand-written domain metrics.
- **OpenTelemetry Collector.** Decouples services from destinations, so trace backends can be
  changed without touching or recompiling service code. Absorbs bursts through its sending queue.
  Owns all collection concerns, including the platform log files and platform metrics that no SDK can
  reach.
- **Prometheus.** Time-series storage plus the query language in which alert expressions are
  written; it is what turns "the system feels slow" into a verifiable threshold.
- **Alertmanager.** Turns a breached rule into a notification, with grouping, silencing, and
  maintenance windows. Without it, alert rules are a file nobody reads.
- **Grafana.** Operational at-a-glance view and visual correlation across metric sources.
- **Elastic APM Server.** Application-level trace view that Jaeger does not provide: service map,
  latency and error evolution per service and per transaction. Accepts OTLP natively, so no
  vendor agent is introduced into the services.
- **Browser SDK.** The only place that can start a trace at the user interaction instead of at the
  first service, and the only vantage point from which failures that never reach a service are
  visible at all: network errors, timeouts, and rejected cross-origin calls. It also carries
  user-perceived timings, which are the closest available proxy for experienced quality.
- **Elasticsearch.** Indexed search over large volumes, which is what finding a correlation
  identifier among millions of lines requires. Also the APM store.
- **Kibana.** Log and APM interface, and the component that makes log-to-trace navigation possible
  because both signals share one store.
- **Fluent Bit.** Flattens OTLP into the shape Logstash can consume natively. See
  [Validation](#validation) for the measured difference; this is the reason it exists in this stack
  and it is deliberately not used as a log tailer.
- **Logstash.** Grok parsing of platform text formats, one place where the field contract is
  defined, a persistent queue so an Elasticsearch outage does not lose logs, and a dead letter queue
  so unparsed logs are visible instead of silently dropped.

### Why Elastic APM instead of Jaeger

Jaeger and Elastic APM overlap almost entirely on distributed trace visualization, so keeping both
means maintaining two trace backends permanently. Elastic APM is preferred because it stores traces
in the Elasticsearch instance that already holds the logs, which is what enables navigation from a
log line to its trace and back. Prometheus is retained for metrics; APM metrics are treated as
complementary and the choice of which series drive dashboards and alerts is left to implementation.

### Why the log plane has two paths

Application logs already leave the process structured and carrying trace context, so routing them
through Logstash would add latency and a failure dependency without changing the data. Platform logs
arrive as unstructured text and need parsing before they satisfy the field contract. The split is by
origin, and it is deliberate rather than incidental: the two sources have different guarantees at the
point of emission.

## Changes from ADR 0012

| Concern | ADR 0012 (baseline) | This ADR |
| --- | --- | --- |
| Trace backend | Jaeger | Elastic APM Server; Jaeger removed |
| Platform logs | Not addressed | Collector `filelog` → Fluent Bit → Logstash → Elasticsearch |
| Platform metrics | Not addressed | Collector component receivers → Prometheus |
| Alert delivery | Rules only | Alertmanager, with rules actually loaded by Prometheus |
| Log normalization | Implicit | Logstash owns the platform field contract |
| Console logging | Enabled at global level | Retained as fallback, restricted to `Warning` and above |

Everything not listed above is carried over unchanged: OpenTelemetry as the instrumentation
standard, the Collector as single ingest point, Prometheus and Grafana for metrics, and
Elasticsearch with Kibana for log centralization and search.

## Alternatives Considered

1. **Fluent Bit tails the platform log files and feeds Logstash directly**
   - Pros: one hop fewer, and no OTLP-to-JSON conversion; the log file itself remains the durable
     buffer while Logstash is unavailable.
   - Cons: collection configuration (mounts, include paths, multiline rules) would live in Fluent
     Bit, so removing Logstash later would require migrating it to the Collector.
   - Rejected in favor of concentrating all collection in the Collector, accepting the extra hop.

2. **Collector exports OTLP/JSON straight to Logstash, without Fluent Bit**
   - Pros: one component fewer.
   - Cons: measured payload is a single object with triple nesting, where the raw line sits at
     `resourceLogs[].scopeLogs[].logRecords[].body.stringValue` and attributes are typed key/value
     arrays. Logstash would need three `split` filters plus attribute unpacking before any grok.
   - Rejected; Fluent Bit performs this flattening natively.

3. **Kafka as the bridge between Collector and Logstash**
   - Pros: durable buffering using the brokers that already exist.
   - Cons: places the log plane on the same brokers as business messaging, and still requires
     flattening nested OTLP inside Logstash.
   - Rejected.

4. **Collector only, using OTTL `ExtractGrokPatterns`, with no Logstash**
   - Pros: fewest components; grok is available in the Collector as of releases after `0.105.0`.
   - Cons: loses Logstash's persistent queue and dead letter queue, which are the reasons Logstash
     was chosen once grok stopped being a differentiator.
   - Rejected, but recorded as the fallback if the Logstash hop proves not to earn its cost.

5. **Fluentd instead of Fluent Bit**
   - Pros: native grok plugin and a far larger plugin catalogue.
   - Cons: heavier runtime, and grok already lives in Logstash in this design, so the advantage does
     not apply.
   - Rejected.

6. **Keeping Jaeger alongside Elastic APM during a comparison window**
   - Pros: allows side-by-side evaluation.
   - Cons: two trace backends tend to become permanent by inertia.
   - Rejected in favor of a single trace backend from the start.

7. **A vendor RUM agent in the browser (Elastic RUM agent, Grafana Faro) instead of the OpenTelemetry
   browser SDK**
   - Pros: richer out of the box, with web vitals, breadcrumbs, and session context available without
     writing instrumentation; less code to own.
   - Cons: introduces a second instrumentation standard, present only in the browser, which breaks the
     rule that vendor choices live in the collection layer and never in application code. An Elastic
     agent would additionally couple the browser to Elastic at source level, so replacing the storage
     layer would mean editing the frontend. Its session-oriented features also pull toward capturing
     end-user identity, which FR-003 forbids.
   - Rejected. The OpenTelemetry browser SDK covers the three signals this project actually needs
     (trace context propagation, client-side failures, user-perceived timings) without either cost.

8. **Loki for centralized logs**
   - Already rejected in ADR 0012 and still rejected, for the same indexed-search reasons.

## Validation

The platform log path was exercised before this ADR was written, using the Collector image currently
pinned in the reference stack (`0.105.0`), Fluent Bit `5.0.9`, and Logstash `8.11.4`, against real
Kafka log lines including a multi-line Java stack trace. Findings that shaped the decisions above:

- **The chain works end to end.** A raw Kafka line arrives in Logstash and comes out with
  `severity_text`, `body`, and `service.name` populated.
- **Fluent Bit's flattening is the reason it is in the stack.** Without it the payload is triple
  nested; with it, the payload is a flat JSON array whose elements the Logstash `json` codec turns
  into one event each, with the raw line in a top-level `log` field.
- **The `filelog` receiver does not join multi-line records by default.** The exception line and its
  `at ...` continuation arrived as two independent records.
- **The Collector parses neither event time nor severity.** Records carried
  `observedTimeUnixNano` but no `timeUnixNano` and no `severityText`, so `@timestamp` defaulted to
  ingest time rather than event time.
- **Fluent Bit starts with memory-only storage.** Durability is not a default.
- **Grok failures are visible rather than silent.** The stack-trace line was tagged
  `_grokparsefailure_kafka` instead of being dropped, which is the dead-letter behaviour Logstash was
  chosen for.

## Implementation Requirements

These follow directly from the validation above and are mandatory for the stack to behave as decided:

- Configure multi-line joining in the `filelog` receiver, since the Collector is the tailer and
  records cannot be reliably rejoined downstream.
- Parse event time and severity from the log body in Logstash, and promote the parsed time to
  `@timestamp`; otherwise every platform log is stamped with its ingest time.
- Enable durable buffering explicitly at each hop: file-backed sending queue in the Collector,
  filesystem buffering in Fluent Bit, and persistent queue plus dead letter queue in Logstash.
- Strip transport noise in Logstash: the `http`, `url`, and `user_agent` fields added by its HTTP
  input, Fluent Bit's `date` field once consumed, and Fluent Bit's `__internal__` block once its
  contents are mapped.
- Keep two separate log pipelines in the Collector so application logs never traverse Logstash.
- Restrict Fluent Bit to platform logs only, so application logs are never indexed twice.
- Load `alert-rules.yml` from `prometheus.yml` and replace the placeholder rule with expressions tied
  to agreed SLI/SLO signals.
- Enable CORS on the Collector's OTLP/HTTP receiver restricted to known origins, since browser
  telemetry is submitted directly by clients.
- Apply client-side sampling to browser telemetry, because its volume scales with users rather than
  with backend traffic and is not bounded by the service fleet.
- Keep end-user identity out of browser payloads; the telemetry hygiene obligation applies to the
  browser exactly as it does to server-side services.

## Consequences

### Positive

- Observability covers the platform as well as the services, closing the log and metric gaps that the
  baseline left open.
- Logs and traces share one store, so incident diagnosis can move from a log line to its trace
  without changing tools.
- Alerts can reach a human, satisfying the constitutional obligation the baseline could not meet.
- Unparsed platform logs become visible instead of silently disappearing.
- A trace spans the whole user operation, and failures visible only to the client stop being invisible
  to the platform.

### Negative / Trade-offs

- Three new operational components (Alertmanager, Fluent Bit, Logstash) plus Elastic APM Server, each
  with its own configuration language and failure modes.
- Accepting telemetry from browsers means an ingest endpoint reachable by untrusted clients, so origin
  restriction and volume control become a security and cost concern rather than a detail.
- The platform log path crosses three hops before storage, so durability depends on buffering being
  configured deliberately at each one.
- Fluent Bit's `__internal__` structure is an implementation-shaped key observed in `5.0.9`; the
  Logstash pipeline is coupled to it and should be re-validated on Fluent Bit upgrades.
- Logstash `8.11.4` defaults to `pipeline.ecs_compatibility: v8`, while the existing index template
  defines a partly custom contract (`body` rather than the ECS `message`, plus `correlationId`).
  The custom contract is retained for continuity; full ECS alignment is deferred and should be a
  separate decision.
