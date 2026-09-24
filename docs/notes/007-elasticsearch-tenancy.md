# 007 - Elasticsearch Tenancy and Shared Fate

**Date:** 2026-07-25
**Status:** Open — accepted for now, revisit when the triggers below appear
**Context:** One Elasticsearch instance currently serves three unrelated tenants. This note records why
that is accepted, the one place it already forced a design choice, and what separating it would cost.
The platform log path itself is decided in [ADR 0013](../adr/0013-definitive-observability-stack.md).

## What shares the instance

| Tenant | What it writes | Written by |
|--------|----------------|------------|
| Business search | `sessions-detected` | Kafka Connect Elasticsearch sink, from `sessions.detected` |
| Observability logs | `observability-logs-*` | Collector (application logs) and Logstash (platform logs) |
| Traces | APM indices | Elastic APM Server |

## Why Elasticsearch is excluded from the platform log path

Every other platform component sends its standard output to Fluent Bit, and Elasticsearch deliberately
does not. Routing its output there would make it the store for its own logs: a record describing an
indexing failure would need that same failing indexing path in order to be readable. Under pressure
this amplifies, because each rejection produces log lines whose delivery depends on the component that
is already struggling.

Its logs therefore stay in the container runtime's local cache and are read with `docker logs`, which
is the deliberate fallback channel rather than an oversight. This does not leave the component
unobserved: its health is collected as metrics through the Collector's Elasticsearch receiver, so
saturation and cluster state are visible in Prometheus and Grafana without depending on its own logs.

## The larger issue: the diagnosis path shares fate with the system

The concern is not only the self-ingest loop. Business indexing and observability ingestion compete for
the same heap, the same I/O, and the same cluster state. Two consequences follow:

- A burst of session indexing degrades log ingestion and trace queries, exactly when volume is high and
  observability matters most.
- If the instance is unavailable, business search and the ability to investigate why it is unavailable
  are lost together. Logstash's persistent queue absorbs a short outage, so records are not lost, but
  they are not queryable while it lasts.

## Options

| Option | Gains | Costs |
|--------|-------|-------|
| **One instance (current)** | Fewest moving parts and the smallest memory footprint, which suits a reference stack | Shared fate as described above |
| **Two instances, business and observability** | Isolates the diagnosis path from the system it observes; each can be sized and retained independently | A second JVM heap, a second index lifecycle to manage, and re-pointing Kibana, the Grafana datasource, and the APM Server at the observability instance |
| **One cluster, separated node roles** | Some resource isolation without a second deployment | Still one cluster state and one failure domain, with more configuration than either alternative |

## When to revisit

Change this when either signal appears: log and trace ingestion visibly competing with session
indexing (queue growth or rejected bulk requests under normal business load), or an incident in which
Elasticsearch being unavailable prevented the investigation of that same incident. Either one turns the
shared instance from an accepted simplification into the cause of the problem.

## Related

- [ADR 0013 - Definitive Observability Stack](../adr/0013-definitive-observability-stack.md) — platform
  log path, and why platform logs bypass the Collector
- [005 - Security Hardening Backlog](./005-security-hardening-backlog.md) — same accept-now-and-record
  approach for reference-stack shortcuts
