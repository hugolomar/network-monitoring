# Research: Device Communication Graph

## Decision 1: Projection identity and idempotent upsert model

- **Decision**: Model communication relationships with identity
  `(sourceIdentity, destinationIdentity, protocol)` and perform idempotent upserts per identity.
- **Rationale**: This preserves structural uniqueness under at-least-once delivery and supports
  deterministic accumulation of `weight`, `firstSeen`, and `lastSeen`.
- **Alternatives considered**:
  - Append-only edge creation per event (rejected: duplicates structure and inflates traversal cost).
  - Identity excluding protocol (rejected: loses protocol-specific communication semantics).

## Decision 2: External host representation

- **Decision**: Represent unresolved destinations as external-host nodes while preserving internal
  device identities as a separate category for retention behavior.
- **Rationale**: Traversals require a unified communication graph, but cleanup and authority differ
  between external and internal identities.
- **Alternatives considered**:
  - Drop unresolved destinations (rejected: loses communication visibility).
  - Treat all destinations as internal devices (rejected: violates inventory authority boundaries).

## Decision 3: Retrieval contract shape and bounding

- **Decision**: Expose one retrieval endpoint returning `{ nodes, edges, truncated }` with configurable
  depth and limit, including enforced caps.
- **Rationale**: Visualization consumers need bounded graph neighborhoods and explicit truncation signal.
- **Alternatives considered**:
  - Unbounded traversal (rejected: unsafe for operator-facing latency and payload size).
  - Server-side only pagination without truncation flag (rejected: weak UX clarity on partial results).

## Decision 4: Retention execution model

- **Decision**: Run retention as an in-process hosted background service every 24 hours, pruning stale
  relationships older than 90 days and then deleting orphan external-host nodes.
- **Rationale**: This keeps lifecycle maintenance close to graph boundary ownership and simple to operate.
- **Alternatives considered**:
  - Manual admin job only (rejected: not reliable for continuous hygiene).
  - Separate scheduler service for this slice (rejected: disproportionate complexity for current scope).

## Decision 5: Outage isolation behavior

- **Decision**: Graph-store failures must be isolated to graph retrieval/projection surfaces, returning
  clear service-unavailable behavior without impacting existing device/session/search endpoints.
- **Rationale**: The graph is additive; failures must not cascade to previously delivered capabilities.
- **Alternatives considered**:
  - Fail-fast entire backend on graph dependency failure (rejected: violates incremental compatibility).
  - Silent degradation without explicit error semantics (rejected: poor diagnosability and operations).

## Decision 6: Testing and verification strategy

- **Decision**: Validate via integration tests focused on projection idempotency, retrieval bounds,
  retention outcomes, and outage isolation, plus manual quickstart checks.
- **Rationale**: Matches constitutional objective-verification requirements and provides reproducible gates.
- **Alternatives considered**:
  - Manual-only verification (rejected: weak regression protection).
  - Unit-only verification (rejected: misses contract and boundary behavior across runtime surfaces).

## Decision 7: Graph endpoint access scope

- **Decision**: Require authentication for graph retrieval; authorize all authenticated roles for this
  feature scope.
- **Rationale**: Satisfies constitutional authN/authZ requirement while avoiding role-fragmentation
  complexity in this increment.
- **Alternatives considered**:
  - Restrict to selected operational roles only (rejected for this increment: added policy complexity).
  - Service-to-service only access (rejected: conflicts with viewer user story scope).

## Decision 8: Projection write retry policy

- **Decision**: Use bounded retries for projection writes; after retry exhaustion, emit operational
  diagnostics and continue with subsequent events.
- **Rationale**: Improves resilience without blocking stream progress or causing indefinite stalls.
- **Alternatives considered**:
  - Unlimited retries (rejected: risk of processing stalls).
  - No retries (rejected: poor resilience to transient failures).

## Decision 9: Observability baseline

- **Decision**: Require structured logs plus counters/latency metrics for projection and retention paths.
- **Rationale**: Supports incident diagnosis and measurable verification without prescribing a specific
  telemetry platform.
- **Alternatives considered**:
  - Logs only (rejected: weak aggregate health visibility).
  - Metrics only (rejected: weak incident-level diagnostic context).
