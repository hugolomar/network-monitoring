# ADR 0006: Avro + Schema Registry for Session Kafka Payloads

- Status: Accepted
- Date: 2026-04-20

## Context

Session events from the probe are intended for Apache Kafka (`sessions.detected` by default). In real
network-monitoring deployments, **event volume can be very high** (many flows, long-lived probes,
fine-grained emissions). Payload encoding therefore affects **broker throughput, network utilization,
storage retention cost, and producer/consumer CPU**.

The platform architecture (see case study) already assumes **Kafka with Schema Registry** and
**Avro/JSON** as supported serializations. We must choose a default for **session** values before
implementing the producer.

## Decision

Use **Avro** (Confluent wire format) for Kafka **message values** of `SessionDetected`, with schemas
**registered and versioned** in **Confluent-compatible Schema Registry**.

- Canonical schema: `specs/001-session-detection/contracts/session-detected-value.avsc`
- Registry subject (value): `sessions.detected-value` (see `session-detected-avro.md`)

## Rationale

- **Efficiency at scale:** Avro is **compact** on the wire compared to JSON with repeated field names;
  for high cardinality and sustained rates, this improves **throughput** and reduces **bandwidth and
  storage** pressure on Kafka.
- **Governed evolution:** Schema Registry enables **compatibility policies** (e.g. backward) so
  producers and consumers can roll out schema changes in a controlled way.
- **Alignment:** Matches the documented platform direction (Kafka + Registry + Avro/JSON) and pairs
  naturally with Elasticsearch sinks and other Registry-aware consumers later.

We explicitly **prioritize efficiency and schema governance** over **raw human readability** of
topic bytes. Operational inspection remains possible via **Registry**, **consumer tooling**, and
parallel **console JSONL** in the probe for validation; debugging Kafka payloads expects **decode
tools**, not eyeballing plain text in the broker.

## Alternatives Considered

1. **JSON (UTF-8) for Kafka values**
   - **Pros:** Easiest to read in ad-hoc tools; simplest first producer.
   - **Cons:** Larger messages and higher serialization cost at scale; evolution is less structured
     unless JSON Schema + Registry discipline is added.
   - **Rejected** as the default for session streaming given expected **high event volume**.

2. **Protobuf**
   - **Pros:** Compact; wide ecosystem.
   - **Cons:** Schema Registry path in this stack is standardized around **Avro/JSON** in the
     architecture narrative; mixing primary encodings without need adds operational surface.
   - **Deferred** unless a future ADR revisits cross-language constraints.

## Consequences

- **Positive:** Better fit for **high-volume** telemetry; clear **versioning** story; consistent with
  Registry-centric operations.
- **Negative:** Every consumer must **deserialize Avro** (and use Registry or an equivalent source of
  truth); operators need tooling to inspect payloads, not raw topic text.
- **Follow-up:** Implement probe `Kafka` publisher with Avro serializer; add dev/staging **Kafka +
  Registry** stack; device events (`002`) will be a **separate** schema and subject when that work
  is specified.
