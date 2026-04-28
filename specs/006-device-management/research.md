# Research: Device Management

## Decision 1: React + TypeScript + Vite SPA

- **Decision**: Implement the Device Management UI as a React + TypeScript + Vite single-page application.
- **Rationale**: The PDF allows a SPA using React/Angular/Vue or a server-rendered UI. A React/Vite SPA is a small, common choice for an operator-facing UI that consumes existing HTTP APIs and can be served as static assets from its own container. TypeScript gives compile-time checks for backend DTO shapes without importing the .NET shared domain.
- **Alternatives considered**:
  - Angular: viable but heavier for a first, narrow UI increment.
  - Vue: viable but no stronger fit than React for this repo.
  - Server-rendered UI: would add server runtime concerns even though the feature only needs browser state over existing HTTP contracts.
  - Blazor/.NET UI: aligns with .NET baseline but couples the UI to the .NET ecosystem more than needed and does not match the PDF's broad SPA guidance.

## Decision 2: UI Uses HTTP DTOs, Not Shared Domain

- **Decision**: Define TypeScript DTO/view models from the existing backend contracts instead of referencing `NetworkMonitoring.Domain`.
- **Rationale**: The UI boundary is HTTP. Backend remains authoritative for validation, persistence, idempotency, and shared domain behavior. Keeping UI models contract-shaped prevents coupling the frontend to backend internals.
- **Alternatives considered**:
  - Reuse/generate from .NET domain types: rejected because the shared domain is not the public UI contract and would leak service internals.
  - Duplicate backend validation rules in the UI: rejected because backend validation is authoritative; the UI may do lightweight form checks only for operator feedback.

## Decision 3: Static Container Deployable

- **Decision**: Package the production UI as static assets served from its own container.
- **Rationale**: The feature requires a separate deployable but no server-side business logic. Static serving keeps runtime simple, containerized, and independent of backend processes.
- **Alternatives considered**:
  - Node.js production server: unnecessary for a static SPA.
  - Serve UI from `NetworkMonitoring.Backend`: rejected because the PDF treats UI as a separate component and this would mix deployables.

## Decision 4: Runtime Backend Base URL Configuration

- **Decision**: Support backend base URL configuration for both local development and reference stack execution.
- **Rationale**: Developers need to run the UI locally against the backend exposed on `localhost:5090`, while the containerized stack may use Compose service names or same-origin proxy/static runtime configuration depending on implementation detail.
- **Alternatives considered**:
  - Hard-code `localhost:5090`: rejected because it does not work from inside containers.
  - Hard-code Compose service DNS: rejected because it does not work for local dev outside Compose.

## Decision 5: Focused UI Tests and Quickstart Validation

- **Decision**: Use focused component/API-client tests for loading, empty inventory, unavailable backend, manual creation, validation rejection, and idempotent/consolidated success, plus Docker/quickstart validation.
- **Rationale**: These tests directly map to the feature's states and contracts without requiring Kafka, Probe, Integration Console, PostgreSQL internals, or Elasticsearch.
- **Alternatives considered**:
  - Full browser E2E in this increment: valuable later but heavier than needed for first UI plan.
  - Backend integration tests from the UI project: rejected because backend already owns contract/persistence tests; UI tests should mock/stub HTTP boundary and quickstart should validate the integrated path.

## Deferred Decisions

- Keycloak login and RBAC enforcement are deferred to a later auth feature.
- mTLS and production transport security are deferred to a later security feature.
- Session search and dashboards are deferred to a later sessions/UI feature.
- Backend contract changes are out of scope; any needed API changes must become a separate backend feature.
