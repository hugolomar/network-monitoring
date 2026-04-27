# Data Model: Device Ingestion

## DeviceDetected Event

Represents one device detection consumed from `devices.detected`.

### Fields

- `eventType`: must be `DeviceDetected`.
- `occurredAtUtc`: event emission timestamp.
- `source`: event producer identifier.
- `schemaVersion`: event contract version.
- `deviceId`: optional upstream identifier; expected to be absent until persistence exists.
- `macAddress`: normalized MAC address and primary device identity.
- `primaryIp`: optional primary IP address.
- `hostname`: optional observed hostname.
- `observedIps`: zero or more observed IP addresses.
- `firstSeenUtc`: first observed timestamp.
- `lastSeenUtc`: latest observed timestamp.
- `discoverySource`: source classification for the detection.

### Validation Rules

- Event value must deserialize according to `devices.detected-value`.
- `eventType` must equal `DeviceDetected`.
- `macAddress` must be present and normalize to the same value as the Kafka key.
- Timestamps must be parseable and `lastSeenUtc` must not be earlier than `firstSeenUtc`.
- Optional IP fields, when present, must be valid IP address values.

## Device Intake Request

Represents the outbound request sent to `POST /devices`.

### Fields

- `macAddress`: normalized MAC address.
- `primaryIp`: optional primary IP address.
- `hostname`: optional hostname.
- `observedIps`: list of observed IP addresses.
- `firstSeenUtc`: first observed timestamp.
- `lastSeenUtc`: latest observed timestamp.
- `discoverySource`: discovery source.
- `sourceEvent`: minimal metadata identifying the source event, including event source and schema
  version.

### Headers

- `Idempotency-Key`: normalized MAC address from the Kafka key/payload identity.

### Validation Rules

- `macAddress` is required.
- `Idempotency-Key` must equal `macAddress`.
- Request body must not include persistence-only fields owned by the future backend.

## Ingestion Attempt

Represents one forwarding attempt for a consumed event.

### Fields

- `macAddress`: normalized MAC identity.
- `attemptNumber`: one-based attempt count.
- `startedAtUtc`: attempt start timestamp.
- `completedAtUtc`: optional completion timestamp.
- `outcome`: `Succeeded`, `RetryableFailure`, `Rejected`, or `RetryExhausted`.
- `statusCode`: optional HTTP status code.
- `reason`: operator-visible reason text.

### State Transitions

- `Pending` -> `Succeeded`: receiver accepts the request.
- `Pending` -> `RetryableFailure` -> `Pending`: transient failure remains within retry limit.
- `Pending` -> `RetryExhausted`: transient failures exceed retry limit.
- `Pending` -> `Rejected`: event validation or receiver validation permanently rejects the request.

## Rejected Event

Represents an event the Integration Console cannot safely forward.

### Reasons

- Deserialization failed.
- Required event field missing or invalid.
- Kafka key missing or malformed.
- Kafka key and payload MAC mismatch.
- Receiver returned permanent validation failure.
- Poison-message handling classified repeated failure as non-forwardable.

### Required Diagnostics

- Topic and partition/offset when available.
- Normalized MAC when available.
- Failure category.
- Human-readable reason.
- Whether the event was skipped, rejected, or exhausted retries.
