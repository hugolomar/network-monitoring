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
- **Telemetry control plane:** OpenTelemetry Collector is the single ingest point for **application**
  telemetry, meaning every signal emitted by code this project owns. Its role expands to also collect
  platform metrics through component-specific receivers (Kafka, Postgres, Elasticsearch, container
  runtime). It is deliberately **not** on the platform log path; see
  [Why platform logs do not enter through the Collector](#why-platform-logs-do-not-enter-through-the-collector).

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
- **Platform log path:** container standard output → Docker logging driver (Forward protocol) →
  **Fluent Bit** (new) → HTTP/JSON → **Logstash** (new) → Elasticsearch.
- **Fluent Bit role:** entry point for platform logs and assembler of multi-line records. It receives
  pushed log events over the Forward protocol; it does **not** read the container runtime's log files
  and does **not** handle metrics.
- **Platform log collection is a push, not a pull.** The container runtime delivers each container's
  standard output to Fluent Bit over a documented protocol. Nothing in this stack reads the runtime's
  on-disk log layout.
- **The log store is excluded from the platform log path.** Elasticsearch's own output is not routed to
  Fluent Bit, because it would then be the store for its own logs: a record describing an indexing
  failure would depend on that same failing path to be readable, and each rejection produces further
  records. Its logs stay in the runtime's local cache, and its health is observed through metrics
  instead. See [note 007](../notes/007-elasticsearch-tenancy.md).
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
  Owns application telemetry ingest and the platform metrics that no SDK can reach. It is not on the
  platform log path; see
  [Why platform logs do not enter through the Collector](#why-platform-logs-do-not-enter-through-the-collector).
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
- **Fluent Bit.** Receives platform logs pushed by the container runtime and reassembles multi-line
  records before anything downstream sees them. Its native record shape is a flat set of fields,
  which is what a text log already is, so nothing has to be wrapped and unwrapped on the way to
  Logstash.
- **Logstash.** Grok parsing of platform text formats, one place where the field contract is
  defined, a persistent queue so an Elasticsearch outage does not lose logs, and a dead letter queue
  so unparsed logs are visible instead of silently dropped.

### Why Elastic APM instead of Jaeger

Jaeger and Elastic APM overlap almost entirely on distributed trace visualization, so keeping both
means maintaining two trace backends permanently. Elastic APM is preferred because it stores traces
in the Elasticsearch instance that already holds the logs, which is what enables navigation from a
log line to its trace and back. Prometheus is retained for metrics; APM metrics are treated as
complementary and the choice of which series drive dashboards and alerts is left to implementation.

### Why platform logs do not enter through the Collector

The Collector is the right entry point for anything our own code emits, and the wrong one for third
party container output. The distinction is not stylistic; it follows from what each source can
produce.

Our services emit through the OpenTelemetry SDK, which is the only layer that knows which trace is
active while a line of code runs. A log record therefore leaves the process already carrying
`traceId`, `spanId`, and the correlation identifier. Routing that through a text-oriented pipeline
would mean discarding structure that exists at the source and attempting to recover it with regular
expressions.

Third party containers have no SDK and never will. Kafka knows nothing about our traces. Their output
is text, and the only available operations on it are capture, assembly, and parsing. For that work
Fluent Bit is the better tool for two measured reasons:

- **It assembles multi-line records at the point of reception.** It maintains buffering state across
  incoming lines and ships maintained parsers for common runtimes, so a Java stack trace is declared
  with a named parser rather than a hand-written start-of-line regular expression per format. The
  Collector performs equivalent joining only inside its file-reading receiver; records that arrive
  over a network protocol are each treated as complete on arrival.
- **Its native model matches the data.** Fluent Bit represents an event as a flat set of fields,
  which is what a text log line plus its container metadata already is. The Collector's native model
  is OTLP, so the same line has to be wrapped into a nested structure and then flattened again before
  Logstash can parse it. That round trip exists only because of the intermediate representation, not
  because of anything the data requires.

The cost of this choice is stated in [Consequences](#consequences): platform logs no longer pass the
Collector, so cross-cutting obligations that would naturally live there have to be enforced on the
platform path in Logstash.

### Why the runtime's log files are not read directly

The alternative mechanism is to have a collector tail the container runtime's own log files and
enrich each record with container metadata obtained from the runtime API. That is the canonical
pattern in the OpenTelemetry documentation and the one Kubernetes deployments use. It is rejected here
for three reasons, none of which depend on the host operating system.

**It depends on a private implementation detail instead of a documented interface.** The on-disk log
layout is not a stable contract: the directory structure, the wrapper format, and the rotation
behavior differ between container runtimes, and within Docker they differ by configured logging
driver. Switching the daemon's driver from `json-file` to `local` changes the encoding entirely and
silently breaks a file-tailing configuration. The Forward protocol, by contrast, is a documented
interface that the runtime is responsible for honoring.

**It requires privileges this project does not otherwise need.** Reading the log directory means
mounting a host path into the collector, and resolving container identifiers into readable service
names means giving the collector access to the runtime's control socket. Access to that socket is
equivalent to root on the host, because whoever can reach it can start a privileged container.
Accepting a permanent host-level privilege for the sake of log labelling is a poor trade when a
mechanism exists that needs no privilege at all.

**Metadata arrives at the source instead of being reconstructed.** In the push model the runtime
attaches the container name, container identifier, and stream to every record as it is emitted. In
the pull model the file path yields only an identifier, and the human-readable name has to be resolved
through API lookups that then need caching and invalidation. The first has no failure mode; the second
has several.

Portability is not a differentiator between the two. Both mechanisms are configured per environment:
file layouts and metadata sources differ across runtimes and orchestrators just as logging drivers do.
What matters for portability is that the contract asked of a platform component is unchanged in every
environment, namely that it writes to standard output, and that the normalization stage, field
contract, and indices downstream of Fluent Bit are identical regardless of how collection is wired.

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
| Platform logs | Not addressed | Runtime logging driver → Fluent Bit → Logstash → Elasticsearch |
| Platform metrics | Not addressed | Collector component receivers → Prometheus |
| Alert delivery | Rules only | Alertmanager, with rules actually loaded by Prometheus |
| Log normalization | Implicit | Logstash owns the platform field contract |
| Console logging | Enabled at global level | Retained as fallback, restricted to `Warning` and above |

Everything not listed above is carried over unchanged: OpenTelemetry as the instrumentation
standard, the Collector as single ingest point for application telemetry, Prometheus and Grafana for
metrics, and Elasticsearch with Kibana for log centralization and search.

## Alternatives Considered

1. **Collector reads the container runtime's log files (`filelog`), with container metadata resolved
   through the runtime API (`docker_observer` and `receiver_creator`)**
   - Pros: the canonical pattern in OpenTelemetry documentation and the closest analogue to how
     Kubernetes deployments collect logs; the log file on disk survives a collector outage, bounded
     only by rotation.
   - Cons: couples the configuration to the runtime's private on-disk layout, which varies by runtime
     and by configured logging driver; requires a host path mount plus access to the runtime control
     socket, which is equivalent to host root; and resolves service names through API lookups that
     need caching and invalidation instead of receiving them with the record. Measured on the pinned
     Collector release, the observer additionally emits one endpoint per published container port, so
     a container with three ports produces three receivers reading the same file, while a container
     with no published ports is not discovered at all.
   - Rejected. See
     [Why the runtime's log files are not read directly](#why-the-runtimes-log-files-are-not-read-directly).

2. **Runtime logging driver → Collector (`fluentforward`) → Fluent Bit → Logstash**
   - Pros: keeps every signal, including platform logs, entering through one process, which is where
     cross-cutting obligations such as telemetry hygiene would naturally be enforced.
   - Cons: measured, the Collector treats each pushed line as a complete record, so a Java stack trace
     arrives as one record per line and has to be reassembled further downstream, where the original
     line order is no longer guaranteed to be recoverable. It also reintroduces the OTLP wrap and
     unwrap round trip, adding a hop whose only purpose is to undo a representation this path never
     needed.
   - Rejected. Multi-line assembly at the point of reception was judged worth more than a single
     ingest process, because a broken stack trace is unusable exactly when it matters most.

3. **Collector exports OTLP/JSON straight to Logstash, without Fluent Bit**
   - Pros: one component fewer.
   - Cons: measured payload is a single object with triple nesting, where the raw line sits at
     `resourceLogs[].scopeLogs[].logRecords[].body.stringValue` and attributes are typed key/value
     arrays. Logstash would need three `split` filters plus attribute unpacking before any grok.
   - Rejected; this is moot once the Collector is off the platform log path, and it is recorded
     because it is what made the OTLP round trip visible as pure overhead.

4. **Kafka as the bridge into Logstash**
   - Pros: durable buffering using the brokers that already exist.
   - Cons: places the log plane on the same brokers as business messaging, so the diagnosis path fails
     together with the system it is meant to diagnose.
   - Rejected.

5. **Platform components write log files into a shared volume that a collector then reads**
   - Pros: works regardless of runtime log layout or logging driver support.
   - Cons: requires changing the logging configuration inside third party components, including
     supplying a replacement `log4j` configuration to the brokers and the connector runtime, and
     replaces standard output with a project-specific convention. It also abandons the container
     contract that a component logs to standard output and the platform decides what happens next.
   - Rejected as a project-specific convention imposed on components that already expose a standard
     one.

6. **Collector only, using OTTL `ExtractGrokPatterns`, with no Logstash**
   - Pros: fewest components; grok is available in the Collector as of releases after `0.105.0`.
   - Cons: loses Logstash's persistent queue and dead letter queue, which are the reasons Logstash
     was chosen once grok stopped being a differentiator.
   - Rejected, but recorded as the fallback if the Logstash hop proves not to earn its cost.

7. **Fluentd instead of Fluent Bit**
   - Pros: native grok plugin and a far larger plugin catalogue.
   - Cons: heavier runtime, and grok already lives in Logstash in this design, so the advantage does
     not apply.
   - Rejected.

8. **Keeping Jaeger alongside Elastic APM during a comparison window**
   - Pros: allows side-by-side evaluation.
   - Cons: two trace backends tend to become permanent by inertia.
   - Rejected in favor of a single trace backend from the start.

9. **A vendor RUM agent in the browser (Elastic RUM agent, Grafana Faro) instead of the OpenTelemetry
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

10. **Loki for centralized logs**
   - Already rejected in ADR 0012 and still rejected, for the same indexed-search reasons.

## Validation

The platform log path was exercised before this ADR was written, using the Collector image currently
pinned in the reference stack (`0.105.0`), Fluent Bit `5.0.9`, and Logstash `8.11.4`, against real
Kafka log lines including a multi-line Java stack trace. Findings that shaped the decisions above:

- **The chain works end to end.** A raw Kafka line arrives in Logstash and comes out with
  `severity_text`, `body`, and `service.name` populated.
- **Neither collector parses event time or severity.** Records arrived without a severity and with a
  timestamp corresponding to ingest, not to the time inside the line, so `@timestamp` defaults to
  ingest time unless the body is parsed downstream. This held for every mechanism tested and is
  therefore not a differentiator.
- **Multi-line behaviour is what separated the two mechanisms.** Receiving pushed logs into Fluent Bit
  with a named multi-line parser produced the exception and both `at ...` continuations as a single
  record. The same input into the Collector, whether read from files or received over the Forward
  protocol, produced one record per line with no association between them.
- **The built-in `java` parser is not the right rule for these formats.** It starts a record at the
  exception line, so a stack trace was assembled correctly but detached from the `ERROR` line that
  reported it, arriving as a separate record with no severity and no event time. Parsers keyed on the
  log layout's own line prefix, treating anything without a leading timestamp as continuation, kept
  the reporting line and its frames together as one event. Multi-line rules therefore belong to the
  log format, not to the language the component is written in.
- **A record that exits immediately can still lose its logs.** With asynchronous delivery, a container
  that emitted lines and exited at once was removed before the runtime flushed its buffer, and nothing
  arrived. Long-lived components are unaffected, and this is the cost of not blocking startup.
- **Container metadata arrives with the record in the push model.** Each event carried the container
  name, the container identifier, and the originating stream, with no runtime API call and without
  mounting the runtime control socket.
- **Fluent Bit's native record shape is already flat.** Fields arrive as `container_name`,
  `container_id`, `source`, and `log`, which the Logstash `json` codec consumes directly. Routed
  through the Collector instead, the same line becomes a triply nested OTLP object whose body sits at
  `resourceLogs[].scopeLogs[].logRecords[].body.stringValue`, requiring a flattening stage that exists
  only to undo the wrapping.
- **The runtime logging driver blocks container startup unless asynchronous delivery is enabled.**
  With synchronous delivery and the receiver down, container creation fails outright. With
  asynchronous delivery the container starts normally, and log events produced while the receiver was
  unavailable were buffered by the runtime and delivered in full once it returned, in order and with
  none missing.
- **Local log inspection is preserved.** `docker logs` continues to work under a non-reading logging
  driver, because the runtime retains a local cache, so the fallback diagnosis channel is not lost.
- **Fluent Bit starts with memory-only storage.** Durability is not a default.
- **Grok failures are visible rather than silent.** The stack-trace line was tagged
  `_grokparsefailure_kafka` instead of being dropped, which is the dead-letter behaviour Logstash was
  chosen for.

## Implementation Requirements

These follow directly from the validation above and are mandatory for the stack to behave as decided:

- Configure multi-line assembly in Fluent Bit with a parser per log layout, keyed on how that layout
  starts a line, since records cannot be reliably rejoined once they are separated. Do not use the
  language-oriented built-in parsers, which detach a stack trace from the line that reported it.
- Capture the message body with a pattern that crosses newlines in Logstash. An assembled record
  carries its frames as embedded newlines, and the default greedy pattern stops at the first one,
  silently truncating every stack trace to its first line.
- Parse event time and severity from the log body in Logstash, and promote the parsed time to
  `@timestamp`; otherwise every platform log is stamped with its ingest time.
- Enable asynchronous delivery on the runtime logging driver, so an unavailable log receiver cannot
  prevent a platform component from starting, and bound its buffer explicitly.
- Enable durable buffering explicitly at each hop: filesystem buffering in Fluent Bit, and persistent
  queue plus dead letter queue in Logstash.
- Apply the logging driver only to platform components. Application containers keep the default
  driver, because their logs already reach Elasticsearch through the SDK and would otherwise be
  indexed twice.
- Derive platform component identity from the container name the runtime supplies with each record,
  normalizing the leading `/` it carries. The Fluent Bit tag stays a routing construct for selecting
  the multi-line parser and is not relied on as a payload field.
- Strip transport noise in Logstash: the `http`, `url`, and `user_agent` fields added by its HTTP
  input, and Fluent Bit's ingest-time `date` field once the event time has been parsed from the body.
- Enforce telemetry hygiene on the platform path inside Logstash. Because platform logs do not pass
  the Collector, this is the only point where redaction can be applied to them, and it is required
  rather than optional: relational store logs can contain statement values.
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
- Platform components are asked for nothing beyond writing to standard output, so no third party
  logging configuration is modified and the container contract is respected.
- The platform log path needs neither a host log directory nor the runtime control socket, so log
  collection does not grant host-level privilege.
- Multi-line records are assembled where the stream is still intact, so stack traces survive the
  pipeline as single events.

### Negative / Trade-offs

- Three new operational components (Alertmanager, Fluent Bit, Logstash) plus Elastic APM Server, each
  with its own configuration language and failure modes.
- Accepting telemetry from browsers means an ingest endpoint reachable by untrusted clients, so origin
  restriction and volume control become a security and cost concern rather than a detail.
- Platform logs do not pass the Collector, so it is no longer a single choke point for every signal.
  Cross-cutting rules have to be expressed twice: once in the Collector for application telemetry and
  once in Logstash for platform logs. This is the main cost of the decision and the reason telemetry
  hygiene on the platform path is listed as a requirement rather than left implicit.
- The runtime's asynchronous send buffer is held in memory and bounded, so an outage longer than the
  buffer allows drops platform log records. Reading log files from disk would instead have retained
  them until rotation. Durability of the first hop is therefore weaker by construction, and the buffer
  size is the knob that decides how much weaker.
- The logging driver is specific to the container runtime and has no equivalent in Kubernetes. Moving
  there means re-wiring collection, though only collection: normalization, the field contract, and the
  indices downstream of Fluent Bit are unchanged.
- Fluent Bit's multi-line assembly depends on named parsers matching the platform formats in use, so a
  platform component changing its log layout can silently start producing split records. This is the
  failure mode to watch on component upgrades.
- The log store is the one platform component whose logs are not centralized, so diagnosing it means
  falling back to the runtime's local cache (`docker logs`) while every other component is queryable in
  one place. Its health is still visible through metrics. More broadly, one Elasticsearch instance now
  serves business search, logs, and traces, which makes the diagnosis path share fate with the system
  it observes; the trade-off and the conditions that would justify separating them are recorded in
  [note 007](../notes/007-elasticsearch-tenancy.md).
- Container runtime metrics require mounting the runtime control socket into the Collector. Access to
  that socket is equivalent to host root, so this is a real privilege grant. It is accepted for
  per-container CPU, memory, and I/O visibility, and deliberately not reused for log collection; the
  two uses of the socket are not equivalent in what they buy.
- Redaction on the platform path is pattern-based and therefore best-effort by nature: it removes
  recognized key-and-value shapes, and cannot recognize a secret that appears as a bare value with no
  surrounding key. It reduces exposure rather than eliminating it, which is one more reason the
  authored services must not log sensitive values in the first place.
- Logstash `8.11.4` defaults to `pipeline.ecs_compatibility: v8`, while the existing index template
  defines a partly custom contract (`body` rather than the ECS `message`, plus `correlationId`).
  The custom contract is retained for continuity; full ECS alignment is deferred and should be a
  separate decision.
