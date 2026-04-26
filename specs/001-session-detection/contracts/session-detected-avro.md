# Contract: SessionDetected Avro + Schema Registry

## Purpose

Define the **Kafka value** encoding for `SessionDetected` events: **Avro** schema, **Schema Registry
subject**, and compatibility expectations. Logical fields and semantics MUST match
[`console-record-schema.md`](./console-record-schema.md); console JSONL remains the human-readable
validation path in parallel.

## Kafka topic (default)

- **`sessions.detected`** (configurable per deployment; see `spec.md` FR-013).

## Schema Registry subject (value)

- **Canonical subject name:** **`sessions.detected-value`**

This follows the usual Confluent **`TopicNameStrategy`**: the value schema for topic
`sessions.detected` is registered under `<topic>-value`. Using the same string as the default topic
avoids mismatches between broker metadata and Registry subjects.

**Alternatives (only if you standardize differently):**

- A global naming prefix, e.g. `network-monitoring.sessions.detected-value`, is valid if **all**
  producers/consumers and CI use the same **subject name strategy** and topic-to-subject mapping;
  the default above is preferred for tooling and onboarding.

## Message key

- The Kafka **key** is **not** described by this Avro record; it is a separate concern (typically a
  `string` or `bytes` encoding of the session identity for partitioning). See `spec.md` FR-014.

## Registry compatibility

- **Recommended config** for subject `sessions.detected-value`: **`BACKWARD`** or
  **`BACKWARD_TRANSITIVE`** (consumers may be on an older schema than the broker’s latest; new
  payloads must remain readable by them until consumers roll forward).
- Any incompatible change MUST be a deliberate **versioning / migration** decision (new subject or
  new major contract), not a silent Registry update.

## Canonical Avro schema

- Source file: [`session-detected-value.avsc`](./session-detected-value.avsc)
- Timestamps (`occurredAtUtc`, `firstSeenUtc`, `lastSeenUtc`) are **ISO-8601 strings** in UTC to
  align with the console contract; a future compatible revision MAY introduce Avro **logical types**
  (e.g. `timestamp-millis`) if all producers and consumers agree.

## Implementation note

- The probe serializes **one Avro record per Kafka message value** using the registered schema for
  subject `sessions.detected-value`. **Device** events are out of scope for this contract; they will
  use a separate topic and subject when specified under `specs/003-device-discovery/`.
