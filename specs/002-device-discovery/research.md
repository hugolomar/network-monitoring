# Research: Device Discovery Separation

## Decision 1: Device discovery as its own increment
- **Decision**: Treat device discovery requirements, contracts, and acceptance criteria as owned by
  this feature slice; other platform capabilities are specified elsewhere.
- **Rationale**: Keeps discovery documentation self-contained and avoids mixing unrelated functional
  areas in one specification.
- **Alternatives considered**:
  - Single umbrella spec for all probe outputs: fewer documents but weaker clarity and harder
    parallel evolution.

## Decision 2: Discovery flow remains probe-local for this increment
- **Decision**: Implement discovery as a dedicated application flow within probe boundaries, using
  existing ports/adapters and explicit validation-result handling.
- **Rationale**: Preserves incremental delivery and avoids coupling this spec to backend persistence.
- **Alternatives considered**:
  - Move consolidation to backend now: architecturally attractive, but outside this feature scope.

## Decision 3: Consolidation semantics in this phase
- **Decision**: Define deterministic discovery consolidation behavior for repeated detections:
  initial detection sets baseline fields, subsequent detections update latest-seen semantics and
  extend observed evidence.
- **Rationale**: Ensures consistent operator-visible behavior and future compatibility with inventory
  reconciliation.
- **Alternatives considered**:
  - Emit every detection as fully independent without consolidation semantics: simpler but noisy and
    less useful for lifecycle interpretation.

## Decision 4: Contract strategy (console now, Kafka-ready)
- **Decision**: Keep structured device detection payload contract stable and explicit in this spec,
  with field-level schema suitable for current console emission and future Kafka publication.
- **Rationale**: Supports contract-first boundaries and minimizes migration friction in next steps.
- **Alternatives considered**:
  - Delay explicit schema until Kafka step: postpones clarity and weakens compatibility checks.

## Decision 5: Validation approach for discovery inputs
- **Decision**: Use explicit validation pattern for expected invalid discovery inputs (aggregate
  errors + continue stream), reserving exceptions for unexpected runtime failures.
- **Rationale**: Improves resilience and maintainability in high-noise observation environments.
- **Alternatives considered**:
  - Exception-driven validation path: simpler initially but weaker operational behavior.

## Execution Notes
- Plan aligns with constitution articles on shared-domain authority, SeedWork immutability,
  contract-first boundaries, and incremental construction with explicit maintainer confirmation for
  potential prior-module impact.

## Implementation Validation (2026-04-19)
- `dotnet test src/NetworkMonitoring.Probe.sln` passed:
  - `NetworkMonitoring.Probe.UnitTests`: 17 passed, 0 failed.
  - `NetworkMonitoring.Probe.IntegrationTests`: 2 passed, 0 failed.
- Discovery validation behavior verified with invalid MAC evidence tests (invalid discovery inputs
  are skipped and stream processing continues).
- Consolidation behavior verified for repeated device detections:
  - deterministic `FirstSeenUtc`/`LastSeenUtc` updates,
  - unique observed IP enrichment,
  - schema-stable serialized `DeviceDetected` payload fields.
