# ADR 0002: Entity Identifier Type as UUID (.NET Guid)

- Status: Superseded by ADR 0004
- Date: 2026-04-06

## Context

The shared domain model uses `SeedWork.Entity` as the identity base for domain entities.  
In this project, entities (`Session`, `Device`) can be created from distributed flows (probe ingestion, backend operations, integration paths), and identifiers must be safe to generate without centralized coordination.

To keep cross-component contracts consistent, the identifier type in the shared base entity must be explicit and stable.

## Decision

Set `Entity.Id` in `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` to UUID as the canonical entity identifier type for the shared domain, implemented as `.NET Guid`.

This means:

- `Entity.Id` is a UUID represented by `.NET Guid`.
- Equality semantics in `Entity` remain based on identifier and concrete type.
- Factories in domain entities may assign `Guid.NewGuid()` when incoming ids are empty.

## Rationale

- **Distributed-safe generation**: new identifiers can be generated locally without database round trips or coordination.
- **Low collision risk**: UUIDs are suitable for multi-service and event-driven flows.
- **Contract consistency**: one identifier type across probe/backend/integration reduces mapping ambiguity.
- **SeedWork alignment**: keeping identity behavior centralized in `Entity` avoids per-entity divergence.

## Alternatives Considered

1. **`int` / `long` (database-generated)**
   - Simpler for relational storage and indexing.
   - Rejected because it couples identity generation to persistence and complicates event-first/distributed creation paths.

2. **`string` identifiers**
   - Flexible for interoperability.
   - Rejected due to weaker type safety and higher risk of inconsistent formatting/validation.

3. **ULID / KSUID**
   - Better sortability and readability in some contexts.
   - Not selected for now to keep baseline simple and aligned with existing .NET ecosystem defaults in current code.

## Consequences

### Positive

- Uniform identity handling across domain entities.
- Easier merge of data coming from multiple producers.
- Reduced dependency on persistence for id assignment.

### Negative / Trade-offs

- UUID/Guid values are less human-friendly than numeric ids.
- Potential index/storage overhead versus incremental numeric keys.
- If ordering by creation time is needed, additional fields (timestamps) remain necessary.

## Governance Note

Because `Entity` lives in `SeedWork`, any future changes to identifier type are architectural and must be handled as an explicit ADR-backed change with migration analysis.

## Governance Exception Record

Applying this ADR required a one-time approved exception to constitution `Article 5 — SeedWork
Immutability`, because `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` was modified.
