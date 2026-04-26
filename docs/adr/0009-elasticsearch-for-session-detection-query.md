# ADR 0009: Elasticsearch for Session Indexing

- Status: Accepted
- Date: 2026-04-20

## Context

Feature **`002-session-indexing`** provides queryable history for session detections emitted by
**`001-session-detection`** to the organization's asynchronous event stream (Kafka, topic
**`sessions.detected`** by default; payloads per ADR **0006**). The indexing feature requires an
agreed query capability so analysts and operators can search and filter past detections (time range,
normalized addresses, ports when applicable, protocol) with bounded results and semantics aligned to
the published session contract — not only live probe output or raw stream consumption.

We need a secondary data system optimized for interactive search over high-volume telemetry, fed from
Kafka, without turning the relational core or Kafka into the primary ad-hoc query engine.

## Decision

Use **Elasticsearch** as the reference implementation for **indexing and querying** session detection
history that originates from Kafka.

- **Ingestion path (reference):** **Kafka Connect** with the **Elasticsearch** sink connector (or an
  organization-approved equivalent) from **`sessions.detected`** (or a derived topic) into one or
  more **Elasticsearch indices** (or **data streams**), with **document shape** mapped from the
  **SessionDetected** value schema (`session-detected-value.avsc`) so query fields match normalized
  session event values and contract semantics.
- **Query surface:** Documented **search API** (HTTP/JSON Query DSL or a thin API that wraps it)
  suitable for operators; **pagination / explicit limits** per the session indexing requirements;
  **access control** per organizational policy.
- **Source of truth:** Kafka remains the **durable event log** for the stream; Elasticsearch is a
  **query-optimized projection**. Eventual consistency / documented lag applies between publication
  and searchability.

Exact cluster sizing, index templates, ILM, and connector configuration belong to implementation and
`specs/002-session-indexing/plan.md` / `quickstart.md`, not to this ADR.

## Rationale

- **Search-first workload:** Session investigations need **range filters**, **term filters** on
  normalized IPs/ports/protocol, and often **free-text or keyword** fields on labels or future
  attributes — Elasticsearch's inverted indices and Query DSL are a direct fit.
- **Volume and append-only pattern:** Detections are largely **append-only** time-series events;
  Elasticsearch handles **high ingest** and **time-based retention** (ILM) patterns common in
  observability platforms.
- **Ecosystem fit:** **Kafka Connect -> Elasticsearch** is a well-trodden path; aligns with ADR
  **0006**'s note on Registry-aware consumers and sinks, reducing bespoke ingestion code for the
  first indexing slice.
- **Operational familiarity:** Elasticsearch (or API-compatible offerings) is widely adopted for
  log and event search, easing on-call and vendor skills reuse.

## Alternatives Considered

1. **PostgreSQL (or other RDBMS) only**
   - **Pros:** Strong consistency; familiar SQL; single stack if already central.
   - **Cons:** Interactive search at telemetry scale often needs careful indexing and tuning;
     high-cardinality filters and exploratory queries can become operationally expensive without a
     design dedicated to search. Rejected as the primary reference for session search in this increment.

2. **ClickHouse / columnar OLAP**
   - **Pros:** Excellent for analytics and scans at scale.
   - **Cons:** Heavier lift for operator-grade ad-hoc search and ecosystem parity with Connect
     sinks; this increment emphasizes investigative filtering more than warehouse-style rollups.
     Deferred unless a future ADR targets analytics cubes.

3. **OpenSearch**
   - **Pros:** API- and use-case similarity to Elasticsearch; open governance.
   - **Cons:** This ADR names Elasticsearch as the decided product for the reference path; if the
     organization standardizes on OpenSearch, treat it as an implementation swap under the same
     search projection decision, with connector and security details updated in plan/quickstart.

4. **No secondary store (consume Kafka only for history)**
   - **Pros:** Minimal moving parts.
   - **Cons:** Does not satisfy the documented query capability for past detections in a bounded,
     operator-friendly way at scale; replay and long retention on Kafka alone are a poor substitute
     for indexed search. Rejected for session indexing.

## Consequences

- **Positive:** Clear query store choice; Connect-based ingest reduces custom pipeline code; aligns
  with session indexing success criteria (sampled query results match contract semantics).
- **Negative:** Additional cluster to run, secure (TLS, auth, RBAC), and upgrade; mapping drift
  between Avro and ES documents must be governed (compat rules, reindex strategy if breaking).
- **Follow-up:** Keep `specs/002-session-indexing/plan.md` and `quickstart.md` aligned with ES +
  Connect, index/data-stream bootstrap, connector configuration, secured query path, and sampling
  verification; document maximum acceptable lag or eventual consistency per the indexing requirements.
