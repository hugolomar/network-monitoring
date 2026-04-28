# Implementation Plan: Device Management

**Branch**: `006-device-management` | **Date**: 2026-04-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/006-device-management/spec.md`

## Summary

Implement a separate browser-based Device Management UI deployable for operators. The UI will consume
only the existing Device Inventory backend HTTP contracts (`GET /devices` and `POST /devices`), render
stored/discovered devices, support manual device creation, and expose clear loading, empty, validation,
idempotent-success, refresh-failure, and backend-unavailable states.

The planned implementation is a standalone React + TypeScript + Vite SPA under `src/NetworkMonitoring.Frontend/`, packaged as its own container and optionally wired into the reference Docker Compose stack. The current feature implements the device management slice inside the broader frontend deployable. The UI remains outside the shared .NET domain model and does not connect directly to Kafka, PostgreSQL, Elasticsearch, the Probe, or the Integration Console.

## Technical Context

**Language/Version**: TypeScript on current Node.js LTS, browser runtime JavaScript  
**Primary Dependencies**: React, Vite, TypeScript, browser Fetch API, lightweight runtime configuration, container static file serving  
**Storage**: N/A for UI persistence; backend remains authoritative for device inventory  
**Testing**: Vitest + React Testing Library for component/API-client behavior; Docker build and manual quickstart validation for deployed UI  
**Target Platform**: Browser UI served from a Linux container and local developer server  
**Project Type**: Separate frontend SPA deployable  
**Performance Goals**: Device inventory page should render representative local inventory quickly enough for operator validation; manual refresh should not require page reload  
**Constraints**: Must consume only `GET /devices` and `POST /devices`; must not use shared .NET domain types; must not access Kafka/PostgreSQL/Elasticsearch directly; auth/RBAC/mTLS/session search/dashboard behavior deferred  
**Scale/Scope**: One device management UI with list, refresh, create form, API client boundary, tests, Docker packaging, Compose integration, and quickstart

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: Confirm whether this feature introduces or updates `Session`,
  `Device`, or related shared entities. If yes, specify inheritance from
  `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` and aggregate root usage.
  - **Status**: Pass. This feature introduces no shared domain entities and does not reference
    `NetworkMonitoring.Domain`. The UI uses HTTP contract DTOs only; backend remains the domain owner.
- **SeedWork Immutability**: Confirm no changes are planned under
  `src/NetworkMonitoring.Domain/SeedWork` except
  `NetworkMonitoring.Domain.csproj` and `GlobalUsings.cs` when strictly necessary.
  - **Status**: Pass. No edits are planned under `src/NetworkMonitoring.Domain/SeedWork`.
- **Boundary Contracts**: List event/API contracts touched and compatibility strategy.
  - **Status**: Pass. The UI consumes existing backend contracts from `005-device-inventory`:
    `GET /devices` and `POST /devices`. No backend, Kafka, or persistence contract changes are planned.
- **Security Controls**: Document authentication/authorization and service transport security.
  - **Status**: Pass with scoped deferral. This increment is local/operator validation UI only. Keycloak
    login, RBAC, and mTLS are explicitly deferred and must be completed before production user-facing
    exposure.
- **Incremental Compatibility Confirmation**: If changes may alter behavior/contracts/assumptions
  of previously delivered modules, record explicit maintainer confirmation before implementation.
  - **Status**: Pass. The UI is a new deployable and consumes existing backend contracts without
    changing Probe, Integration Console, Kafka, PostgreSQL, Elasticsearch, or backend behavior.
- **Verification Path**: Define objective validation for each critical requirement.
  - **Status**: Pass. Validation includes component/API-client tests, UI state tests, Docker build,
    local dev quickstart, and reference stack quickstart against the existing backend.

## Project Structure

### Documentation (this feature)

```text
specs/006-device-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── NetworkMonitoring.Domain/
├── NetworkMonitoring.Probe/
├── NetworkMonitoring.IntegrationConsole/
├── NetworkMonitoring.Backend/
└── NetworkMonitoring.Frontend/
    ├── src/
    │   ├── api/
    │   ├── components/
    │   ├── config/
    │   ├── models/
    │   ├── pages/
    │   └── test/
    ├── public/
    ├── Dockerfile
    ├── package.json
    ├── tsconfig.json
    ├── vite.config.ts
    └── index.html
```

**Structure Decision**: Add one separate frontend deployable under `src/NetworkMonitoring.Frontend/`. The `006-device-management` feature is the first slice in that deployable; future UI slices such as sessions, dashboards, and login can live in the same frontend without creating one UI project per feature. This keeps the UI aligned with the PDF's separate web application component, avoids coupling it to backend internals, and preserves the existing .NET services unchanged.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Frontend uses TypeScript/React rather than .NET 10 | The PDF explicitly allows SPA UI technologies and the feature is a browser UI, not a domain/backend service | A .NET-only UI would add server-side coupling and is less direct for a small SPA consuming HTTP contracts |
