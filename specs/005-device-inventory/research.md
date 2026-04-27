# Research: Device Inventory

## Decision 1: Separate backend/API deployable

- **Decision**: Implement device inventory as a separate `NetworkMonitoring.Backend`
  backend/API service.
- **Rationale**: The PDF describes the backend as the owner of a single authoritative device
  inventory. A separate deployable preserves the existing probe and Integration Console boundaries and
  gives the inventory its own runtime, persistence, API contract, and container package.
- **Alternatives considered**:
  - Add inventory endpoints to the Integration Console: rejected because the Integration Console is an
    event-to-HTTP bridge, not the authoritative inventory owner.
  - Add device inventory to the probe: rejected because the probe must remain capture/publish focused.

## Decision 2: Explicit clean/hexagonal backend layers

- **Decision**: Use `Application`, `Infrastructure`, and `Host` layers within the backend project,
  while reusing the existing shared `NetworkMonitoring.Domain` project as the domain authority.
- **Rationale**: Application use cases need to be testable without HTTP or persistence; persistence and
  HTTP are adapters. This mirrors the established probe and Integration Console style.
- **Alternatives considered**:
  - Direct database calls from endpoint handlers: rejected because it bypasses shared validation and
    makes idempotency/consolidation harder to test.
  - Backend-local domain classes: rejected because the PDF requires a shared domain DLL and the repo
    already defines `Device`, `MacAddress`, `IpAddress`, and `DiscoverySource`.

## Decision 3: Active shared domain usage

- **Decision**: Backend business logic must actively construct and use shared `Device`, `MacAddress`,
  `IpAddress`, and `DiscoverySource` types when accepting and consolidating intake requests.
- **Rationale**: A project reference alone is not enough. The backend is the authoritative validation
  point for both automatic and future manual device creation, so it must use the same invariants as the
  probe and Integration Console.
- **Alternatives considered**:
  - Validate with backend-only DTO rules: rejected because it would duplicate normalization and drift
    from the shared model.

## Decision 4: PostgreSQL for local and future inventory persistence

- **Decision**: Use PostgreSQL as the device inventory datastore, accessed behind an Application
  repository port from Infrastructure/Persistence.
- **Rationale**: The PDF names PostgreSQL/TimescaleDB for backend storage. PostgreSQL fits local
  Docker Compose validation, supports unique constraints for normalized MAC identity, and remains
  suitable for future backend/UI growth.
- **Alternatives considered**:
  - In-memory store: rejected because devices must survive backend process restart.
  - File-based JSON/SQLite: simpler locally, but weaker alignment with the PDF's backend database
    direction and less representative for future deployment.
  - Elasticsearch: rejected for authoritative inventory because it is already positioned as a search
    projection for sessions, not the source of truth for devices.

## Decision 5: Preserve `POST /devices` intake contract

- **Decision**: Implement `POST /devices` as the compatibility baseline defined in
  `specs/004-device-ingestion/contracts/device-intake-http.md`.
- **Rationale**: The Integration Console already forwards this contract. Preserving it lets `004` run
  against the real backend without Kafka/probe changes.
- **Alternatives considered**:
  - Redesign the intake request during backend implementation: rejected unless a compatibility-safe
    refinement is explicitly documented before tasks.

## Decision 6: `GET /devices` as minimal inventory read contract

- **Decision**: Add `GET /devices` to return the stored inventory for validation and future UI work.
- **Rationale**: The PDF explicitly lists `GET /devices -> list devices`, and `006-device-ui` needs a
  read surface. Keeping this endpoint list-only avoids UI scope creep while making persistence
  verifiable.
- **Alternatives considered**:
  - Defer all read endpoints to `006-device-ui`: rejected because backend persistence would be harder
    to validate and the PDF assigns listing to the backend API.

## Decision 7: Idempotency and uniqueness by normalized MAC

- **Decision**: Treat normalized MAC as the stable device identity and require `Idempotency-Key` to
  match the request body MAC for intake.
- **Rationale**: The Integration Console contract already uses normalized MAC as idempotency identity.
  PostgreSQL uniqueness on normalized MAC plus Application-level consolidation prevents duplicates.
- **Alternatives considered**:
  - Store every intake event separately as a new device row: rejected because it violates the single
    authoritative inventory and uniqueness/consistency rules.
  - Use generated request IDs only: rejected because retries and duplicate detections need stable
    device identity, not per-request identity.

## Decision 8: Security scope for this increment

- **Decision**: Keep login/RBAC implementation out of this increment while documenting that production
  exposure requires endpoint protection aligned with the later UI/security work.
- **Rationale**: The user explicitly scoped login/RBAC out. `005` is focused on inventory behavior and
  local Integration Console validation. The plan keeps security visible without expanding scope.
- **Alternatives considered**:
  - Implement Keycloak/RBAC now: rejected as scope creep into identity/UI concerns.

## Decision 9: Validation strategy

- **Decision**: Validate through direct HTTP/API tests, persistence/idempotency integration tests,
  architecture dependency checks, Docker build validation, and an Integration Console forwarding
  scenario against the real backend.
- **Rationale**: This covers the core user stories independently while proving the 004 -> 005 handoff.
- **Alternatives considered**:
  - Only unit-test use cases: rejected because the feature is defined by external HTTP and persistence
    contracts.

## Implementation Validation Notes

- `dotnet test src/NetworkMonitoring.sln`: passed after implementing `NetworkMonitoring.Backend`
  unit/integration tests. Kafka-gated tests remained skipped unless their environment flags are set.
- PostgreSQL-backed integration validation: reference stack now includes `postgres` and
  `network-monitoring-backend`; `docker compose -f docker-compose.reference-stack.yml config` passed.
- `NetworkMonitoring.Backend` Docker build validation: `docker build -f src/NetworkMonitoring.Backend/Dockerfile -t network-monitoring-backend:local .` passed.
