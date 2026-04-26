# ADR 0005: Nullable `Entity.Id` Until Persistence Assigns a Value

- Status: Accepted
- Date: 2026-04-19
- Amends: [ADR 0004](0004-entity-id-revert-to-integer.md) (integer family unchanged; nullability added)

## Context

Shared entities (`Session`, `Device`) inherit `Id` from `SeedWork.Entity`. The probe creates
instances before any database exists in the current increments. We need a clear meaning for
“identifier not yet issued by persistence” without overloading a numeric sentinel.

[ADR 0004](0004-entity-id-revert-to-integer.md) replaced **`Guid`** with **integer-oriented**
identifiers in `Entity`. This ADR does **not** revisit that choice: it only refines the **CLR
representation** from non-nullable **`int`** to **`int?`** so “unset” is explicit.

## Decision

Change `Entity.Id` to **`int?`** (`Nullable<int>`):

- **`null`**: no persistent identifier yet (typical for probe-emitted entities).
- **Positive integer**: assigned by infrastructure (e.g. database identity) when that layer exists.

`Session` and `Device` factories use `ResolvePersistentId`: only **`id > 0`** is stored; `null` or
non-positive values keep **`Id` null**.

`IsTransient()` is defined as **`!Id.HasValue`**.

## Alternatives Considered

1. **Keep `int Id` and use `0` as “no id”** (common with EF Core before save).
   - Avoids editing `SeedWork.Entity`.
   - Rejected here to avoid conflating “unset” with a valid numeric value and to align JSON
     `sessionId` / `deviceId` with **`null`** in outbound contracts without special casing `0`.

2. **Do not inherit persistence id from `Entity` for probe DTOs only**
   - Rejected to keep a single identity model on the aggregate.

## Consequences

### Positive

- Explicit semantics for “not persisted yet”.
- Console/Kafka payloads can emit **`null`** for ids until storage assigns them.

### Negative / Trade-offs

- **`SeedWork/Entity.cs` is modified** (same class of change as past ADR 0002/0004). Treat as a
  **documented, intentional** evolution of the identity model; future changes to SeedWork still
  require constitution / review as usual.
- Call sites must handle **`int?`** (null checks or `??` where a non-null id is required).

## Related

- [ADR 0004](0004-entity-id-revert-to-integer.md): **Guid → integer family** (this ADR keeps that;
  adds **`?`** for transient entities).
- Specs: `specs/001-session-detection/data-model.md`, `specs/002-device-discovery/data-model.md`,
  and console schemas for nullable `sessionId` / `deviceId`.
