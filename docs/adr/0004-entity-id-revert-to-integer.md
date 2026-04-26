# ADR 0004: Revert Entity Identifier Type to Integer

- Status: Accepted
- Date: 2026-04-06
- Supersedes: ADR 0002
- Amended by: [ADR 0005](0005-entity-id-nullable-until-persistence.md) (nullable `int?` for unset id)

## Context

ADR 0002 established UUID/Guid as the canonical identifier type in `SeedWork.Entity`.
After implementation review, the team identified that near-term persistence strategy is relational
and expected to rely on integer-oriented identity conventions.

To reduce integration friction in upcoming persistence work, the identifier strategy was revisited.

## Decision

Revert `Entity.Id` in `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` from UUID/Guid to the
**integer family** (relational-friendly), implemented in code as **`int`** at the time of this ADR.

The shared domain entities continue to inherit identity from `Entity` without local identifier-type
overrides.

**Amendment:** [ADR 0005](0005-entity-id-nullable-until-persistence.md) changes the concrete CLR type
to **`int?`** so “not yet persisted” is represented as **`null`** instead of overloading `0`. The
substance of **this** ADR remains: **identifiers are integer-oriented, not `Guid`**. For the current
shape of `Entity.Id`, see ADR 0005 and `SeedWork/Entity.cs`.

## Rationale

- Better fit with expected relational persistence approach in the current project phase.
- Simpler mapping and operational conventions for repository/database layers.
- Keeps identity ownership centralized in `SeedWork.Entity`.

## Consequences

### Positive

- Reduced friction for database integration.
- Simpler persistence-centric query/index patterns.

### Negative / Trade-offs

- Loses built-in distributed uniqueness of UUID/Guid.
- May require future migration if distributed id generation becomes a hard requirement.

## Documentation Alignment

Feature specs/plans/contracts should reference identity by inheritance from `Entity` rather than
duplicating concrete primitive type in multiple places.

## Governance Exception Record

The one-time approved exception to constitution `Article 5 — SeedWork Immutability` occurred when
ADR 0002 modified `src/NetworkMonitoring.Domain/SeedWork/Entity.cs`.

This ADR reverts that change and restores the intended SeedWork baseline. The exception is
considered closed and does not relax the rule for future changes.
