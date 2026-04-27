# Implementation Plan: Device Inventory

**Branch**: `005-device-inventory` | **Date**: 2026-04-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/005-device-inventory/spec.md`

**Note**: This plan covers the backend-owned device inventory only. UI, login/RBAC rollout,
probe-side discovery, Integration Console Kafka consumption, and session backend behavior remain
outside this increment.

## Summary

Implement a separate `NetworkMonitoring.Backend` backend/API deployable that accepts the
`POST /devices` intake contract established by `004-device-ingestion`, validates and consolidates
devices using the shared `NetworkMonitoring.Domain` model, persists one authoritative device inventory
record per normalized MAC, and exposes `GET /devices` for the later UI feature.

The implementation will follow clean/hexagonal boundaries: Application owns intake/listing use cases
and repository ports; Infrastructure owns PostgreSQL persistence and mappings; Host/API owns HTTP
endpoints, DTO boundary validation, dependency injection, configuration, and container startup.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: ASP.NET Core hosting, shared `NetworkMonitoring.Domain`, EF Core PostgreSQL provider, Options/configuration, structured logging  
**Storage**: PostgreSQL for local development and future extension; backend persists authoritative device inventory records and idempotency/consolidation state  
**Testing**: xUnit unit tests, API contract/integration tests, persistence integration tests,
Integration Console forwarding validation  
**Target Platform**: Linux/containerized backend API process  
**Project Type**: Separate deployable backend/API service (`NetworkMonitoring.Backend`)  
**Performance Goals**: Direct validation should handle representative local ingestion bursts without
creating duplicate device records; query path should return local inventory promptly for operator/UI
validation  
**Constraints**: Preserve `POST /devices` contract from `004-device-ingestion`; one logical stored
device per normalized MAC; use shared domain types actively; no UI, login/RBAC rollout, probe changes,
session backend, or Kafka contract changes  
**Scale/Scope**: One bounded device inventory service with intake, consolidation, persistence,
listing, diagnostics, container packaging, and validation paths

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: Confirm whether this feature introduces or updates `Session`,
  `Device`, or related shared entities. If yes, specify inheritance from
  `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` and aggregate root usage.
  - **Status**: Pass. This feature reuses existing `Device`, `MacAddress`, `IpAddress`, and
    `DiscoverySource` from `NetworkMonitoring.Domain`. It must not duplicate those concepts in the
    backend and does not introduce new shared domain entities.
- **SeedWork Immutability**: Confirm no changes are planned under
  `src/NetworkMonitoring.Domain/SeedWork` except
  `NetworkMonitoring.Domain.csproj` and `GlobalUsings.cs` when strictly necessary.
  - **Status**: Pass. No SeedWork edits are planned.
- **Boundary Contracts**: List event/API contracts touched and compatibility strategy.
  - **Status**: Pass. The backend implements the existing `POST /devices` contract from
    `004-device-ingestion` and adds `GET /devices` as a new read contract for the future UI. The
    `DeviceDetected` Kafka contract remains unchanged.
- **Security Controls**: Document authentication/authorization and service transport security.
  - **Status**: Pass with scoped deferral. This increment is not the UI/RBAC rollout; endpoints are
    built for local/backend validation and documented as needing production auth/RBAC before
    user-facing exposure. Service transport/security configuration remains a planning concern, but
    Keycloak login/RBAC implementation is explicitly deferred.
- **Incremental Compatibility Confirmation**: If changes may alter behavior/contracts/assumptions
  of previously delivered modules, record explicit maintainer confirmation before implementation.
  - **Status**: Pass. The plan preserves Integration Console forwarding semantics and does not require
    probe or Kafka publication changes. Any compatibility-safe refinement to `POST /devices` must be
    explicitly documented before implementation.
- **Verification Path**: Define objective validation for each critical requirement.
  - **Status**: Pass. Validation will include direct HTTP/API tests, persistence/idempotency tests,
    architecture dependency checks, Docker build validation, and Integration Console forwarding to the
    real backend.

## Project Structure

### Documentation (this feature)

```text
specs/005-device-inventory/
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
└── NetworkMonitoring.Backend/
    ├── Application/
    │   ├── Models/
    │   ├── Ports/
    │   └── UseCases/
    ├── Infrastructure/
    │   └── Persistence/
    ├── Host/
    │   ├── DependencyInjection/
    │   └── Endpoints/
    ├── Dockerfile
    ├── appsettings.json
    └── NetworkMonitoring.Backend.csproj

tests/
├── NetworkMonitoring.Probe.UnitTests/
├── NetworkMonitoring.Probe.IntegrationTests/
├── NetworkMonitoring.IntegrationConsole.UnitTests/
├── NetworkMonitoring.IntegrationConsole.IntegrationTests/
├── NetworkMonitoring.Backend.UnitTests/
└── NetworkMonitoring.Backend.IntegrationTests/
```

**Structure Decision**: Add one separate deployable backend/API project for the device inventory and
two test projects. This is proportional because the feature has its own runtime, HTTP surface,
persistence adapter, configuration, and container image while still sharing `NetworkMonitoring.Domain`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|

## Post-Design Constitution Check

- **Shared Domain Integrity**: Pass. The design reuses existing shared domain types and explicitly
  forbids backend-only duplicates for device identity/value objects.
- **SeedWork Immutability**: Pass. No SeedWork changes are required by the design.
- **Boundary Contracts**: Pass. `POST /devices` remains compatible with `004-device-ingestion`;
  `GET /devices` is documented as a new read contract for validation and future UI.
- **Security Controls**: Pass with documented scope. Login/RBAC is intentionally deferred and the
  quickstart/contract notes avoid presenting the endpoint as production user-facing without later
  protection.
- **Incremental Compatibility Confirmation**: Pass. Existing probe, Kafka, and Integration Console
  Kafka consumption behavior remain unchanged.
- **Verification Path**: Pass. The plan defines direct HTTP validation, persistence/idempotency tests,
  Integration Console forwarding validation, `dotnet test src/NetworkMonitoring.sln`, Docker build
  validation, and architecture dependency checks.
