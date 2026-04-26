# Contract: Console Record Schema

## Purpose
Define the structured console output format used to validate probe detection before Kafka
integration.

## Encoding
- UTF-8 text
- One JSON object per line (JSONL)

## Common Envelope Fields
- `eventType`: `"SessionDetected"`
- `occurredAtUtc`: ISO-8601 timestamp
- `source`: `"probe"`
- `schemaVersion`: integer (starts at `1`)

## SessionDetected Payload
- `sessionId` (integer|null; `null` until a persistent id exists, e.g. after storage in a later increment)
- `sourceIp` (string)
- `destinationIp` (string)
- `sourcePort` (number|null)
- `destinationPort` (number|null)
- `protocol` (string)
- `firstSeenUtc` (string timestamp)
- `lastSeenUtc` (string timestamp)
- `bytesObserved` (number)

## Validation Expectations
- Records that fail required-field validation are not emitted as entity events.
- Validation errors are surfaced as warning diagnostics with enough context to identify
  the dropped observation.
- Repeated traffic matching the same session identity within the configured deduplication window
  may result in **fewer** `SessionDetected` lines than raw observations (see `001` functional
  requirements for session emission suppression).
