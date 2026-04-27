# Contract: DeviceDetected Avro + Schema Registry

## Purpose

Define the Kafka value encoding for `DeviceDetected` events: Avro schema, Schema Registry subject, and
compatibility expectations. Logical fields and semantics MUST match
[`device-detected-schema.md`](./device-detected-schema.md); console JSONL remains the
operator-visible validation path in parallel.

## Kafka topic (default)

- **`devices.detected`** (configurable per deployment; see `spec.md` FR-014).

## Schema Registry subject (value)

- **Canonical subject name:** **`devices.detected-value`**

This follows Confluent `TopicNameStrategy`: the value schema for topic `devices.detected` is
registered under `<topic>-value`.

## Message key

- The Kafka key is not described by this Avro record; it is a separate string key.
- The key MUST be the normalized MAC address from the payload (`macAddress`), per `spec.md` FR-015.

## Registry compatibility

- Recommended config for subject `devices.detected-value`: `BACKWARD` or `BACKWARD_TRANSITIVE`.
- Any incompatible change MUST be a deliberate versioning or migration decision, not a silent Registry
  update.

## Canonical Avro schema

- Source file: [`device-detected-value.avsc`](./device-detected-value.avsc)
- Timestamps (`occurredAtUtc`, `firstSeenUtc`, `lastSeenUtc`) are ISO-8601 strings in UTC to align
  with the console contract.

## Implementation note

- The probe serializes one Avro record per Kafka message value using the registered schema for subject
  `devices.detected-value`.
- Publication follows the same validation, consolidation, and device deduplication behavior as console
  `DeviceDetected` output.
