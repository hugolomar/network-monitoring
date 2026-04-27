# Data Model: Device Inventory

## Device Intake Request

Represents the inbound request sent by the Integration Console to `POST /devices`.

### Fields

- `Idempotency-Key` header: required normalized MAC identity.
- `macAddress`: required normalized MAC address in the request body.
- `primaryIp`: optional primary observed IP.
- `hostname`: optional hostname.
- `observedIps`: zero or more observed IP addresses.
- `firstSeenUtc`: required first observed timestamp.
- `lastSeenUtc`: required latest observed timestamp.
- `discoverySource`: required discovery source.
- `sourceEvent`: optional diagnostic metadata from the originating `DeviceDetected` event.

### Validation Rules

- Header `Idempotency-Key` is required and must normalize to a valid MAC address.
- Body `macAddress` is required and must normalize to the same MAC as `Idempotency-Key`.
- Optional IP fields must be valid IP addresses when present.
- `lastSeenUtc` must be greater than or equal to `firstSeenUtc`.
- Request validation must construct/use shared domain types rather than backend-only identity logic.

## Stored Device

Represents the authoritative inventory record for one normalized MAC identity.

### Fields

- `id`: backend-managed persistent identifier.
- `macAddress`: normalized MAC identity, unique per stored device.
- `primaryIp`: optional current primary IP.
- `hostname`: optional latest known hostname.
- `observedIps`: unique set of observed IP evidence.
- `firstSeenUtc`: earliest accepted observation timestamp.
- `lastSeenUtc`: latest accepted observation timestamp.
- `discoverySource`: latest accepted discovery source.
- `createdAtUtc`: backend-managed creation timestamp.
- `updatedAtUtc`: backend-managed update timestamp.

### Relationships

- One `Stored Device` is keyed by one normalized MAC address.
- One `Stored Device` may have zero or more observed IP values.
- Intake requests for the same normalized MAC consolidate into the same stored device.

### Validation Rules

- `macAddress` must be unique.
- `observedIps` must not contain duplicate normalized IP values.
- Timestamp consolidation preserves earliest `firstSeenUtc` and latest `lastSeenUtc`.
- `hostname` and `primaryIp` represent latest known non-null values. A new intake request updates either
  value only when the incoming value is non-null and the incoming `lastSeenUtc` is later than the
  stored `lastSeenUtc`.
- If the stored `hostname` or `primaryIp` is null and the incoming value is non-null, the incoming value
  fills the missing value even when the incoming `lastSeenUtc` is equal to or earlier than the stored
  `lastSeenUtc`.
- If the incoming `lastSeenUtc` equals the stored `lastSeenUtc` and both stored and incoming values are
  non-null but different, the stored value is retained to keep repeated processing deterministic.
- Null incoming `hostname` or `primaryIp` values never clear existing stored values.
- Shared `Device` invariants must be used to validate and consolidate inventory state.

### Persistence Schema Rules

- PostgreSQL schema is managed by EF Core migrations committed with the backend project under
  `src/NetworkMonitoring.Backend/Infrastructure/Persistence/Migrations/`.
- Local development and automated integration validation apply pending migrations before intake/query
  validation so accepted devices survive backend process restart.
- Production-like environments should run the same migrations explicitly as a deployment step rather
  than relying on an in-memory or ad hoc schema.

## Idempotency Identity

Represents the device intake identity used to make repeated intake safe.

### Fields

- `key`: normalized value from `Idempotency-Key`.
- `macAddress`: normalized body MAC address.
- `receivedAtUtc`: timestamp when the backend processed the request.

### Validation Rules

- `key` and `macAddress` must match after normalization.
- Duplicate intake for the same normalized MAC must not create a second stored device.
- Concurrent duplicate intake must resolve to a single authoritative stored device.

## Rejected Intake

Represents an intake request that cannot safely change inventory state.

### Reasons

- Malformed, empty, or unsupported request body.
- Missing, blank, malformed, or mismatched `Idempotency-Key`.
- Invalid MAC or IP fields.
- Missing required fields.
- Invalid timestamp ordering.
- Persistence failure before state can be committed.

### Required Diagnostics

- Failure category.
- Normalized MAC when safely available.
- HTTP outcome classification.
- Human-readable reason.
- Whether inventory state was unchanged.

## Device Inventory Result

Represents the read model returned by `GET /devices`.

### Fields

- `id`: backend persistent identifier.
- `macAddress`: normalized MAC identity.
- `primaryIp`: optional primary IP.
- `hostname`: optional hostname.
- `observedIps`: observed IP evidence.
- `firstSeenUtc`: earliest accepted observation timestamp.
- `lastSeenUtc`: latest accepted observation timestamp.
- `discoverySource`: current discovery source.

### Validation Rules

- Each normalized MAC appears at most once in the result set.
- Empty inventory returns an empty result, not an error.
