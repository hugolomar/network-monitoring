# Contract: DeviceDetected Schema

## Encoding
- Console mode: UTF-8 text, one JSON object per line (JSONL).
- Kafka mode: Avro value registered in Schema Registry under subject `devices.detected-value`; see
  `device-detected-avro.md` and `device-detected-value.avsc`.

## Envelope Fields
- `eventType`: `"DeviceDetected"`
- `occurredAtUtc`: ISO-8601 timestamp (instant the record is emitted; observation-derived times are
  `firstSeenUtc` / `lastSeenUtc` on the payload)
- `source`: `"probe"`
- `schemaVersion`: integer (starts at `1`)

## Payload Fields
- `deviceId` (integer|null; `null` until a persistent id exists, e.g. after storage in a later increment)
- `macAddress` (string, normalized)
- `primaryIp` (string|null)
- `hostname` (string|null)
- `observedIps` (array of strings)
- `firstSeenUtc` (string timestamp)
- `lastSeenUtc` (string timestamp)
- `discoverySource` (string)

## Validation Expectations
- Records missing required identity/lifecycle fields are not emitted.
- Invalid-source observations surface diagnostics and processing continues.
- Repeated traffic for the same normalized MAC may produce **fewer** `DeviceDetected` lines than raw
  observations when `DeviceDeduplicationWindowMinutes` is positive (see `spec.md` FR-012).
- Kafka publication uses the same validation and deduplication semantics as console output.

## Compatibility Rule
- Any schema-breaking change requires explicit contract versioning and compatibility declaration.
