# Implementation Plan: Device Ingestion

**Branch**: `004-device-ingestion` | **Date**: 2026-04-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/004-device-ingestion/spec.md`

**Note**: This plan covers only Integration Console ingestion. The real Devices backend, persistence,
UI, login/RBAC, and probe-side discovery changes remain outside this increment.

## Summary

Implement a separate Integration Console worker that consumes `DeviceDetected` Avro events from
Kafka topic `devices.detected`, validates the event identity, maps valid detections to a stable
`POST /devices` intake contract, and forwards them to a configurable backend base URL. Because the
real backend is deferred to `005-device-inventory`, validation uses a fake/test HTTP receiver while
preserving the downstream contract and idempotency expectations.

The design follows the existing probe patterns: .NET Worker hosting, Application ports/use cases,
Infrastructure adapters for Kafka/Schema Registry and HTTP, shared-domain value objects for MAC/IP
normalization where useful, and opt-in integration validation against the reference Kafka stack.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: .NET Worker hosting, shared `NetworkMonitoring.Domain`, Confluent.Kafka,
Confluent.SchemaRegistry Avro serdes, `HttpClient`/factory-style outbound HTTP, structured logging  
**Storage**: N/A in this increment; no database or durable ingestion store is implemented  
**Testing**: xUnit unit tests, fake/test HTTP receiver tests, gated Kafka integration tests with
`RUN_KAFKA_INTEGRATION=1` and the reference stack  
**Target Platform**: Linux/containerized worker process  
**Project Type**: Separate deployable worker/console process (`NetworkMonitoring.IntegrationConsole`)  
**Performance Goals**: Process sampled `DeviceDetected` events continuously without blocking later
valid events behind malformed or permanently rejected events  
**Constraints**: Preserve `DeviceDetected` contract from `003-device-discovery`; do not implement real
backend/API, persistence, UI, login/RBAC, or probe-side discovery changes  
**Scale/Scope**: One bounded ingestion service: consume `devices.detected`, validate/map events, and
forward `POST /devices` to a configurable receiver with retries and idempotency identity

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: Confirm whether this feature introduces or updates `Session`,
  `Device`, or related shared entities. If yes, specify inheritance from
  `src/NetworkMonitoring.Domain/SeedWork/Entity.cs` and aggregate root usage.
  - **Status**: Pass. No shared domain entity is introduced or changed. Existing value objects such as
    `MacAddress`, `IpAddress`, and `DiscoverySource` may be reused for validation/normalization only.
- **SeedWork Immutability**: Confirm no changes are planned under
  `src/NetworkMonitoring.Domain/SeedWork` except
  `NetworkMonitoring.Domain.csproj` and `GlobalUsings.cs` when strictly necessary.
  - **Status**: Pass. No SeedWork changes are planned.
- **Boundary Contracts**: List event/API contracts touched and compatibility strategy.
  - **Status**: Pass. The input event contract is existing `DeviceDetected` on `devices.detected`
    (`devices.detected-value`) and must remain unchanged. The output contract is a new proposed
    `POST /devices` intake contract documented under this feature and validated with a fake receiver.
- **Security Controls**: Document authentication/authorization and service transport security.
  - **Status**: Pass. Transport/security settings for Kafka, Schema Registry, and backend HTTP are
    configurable. Production TLS/mTLS is supported by configuration and full user/RBAC concerns remain
    outside this non-user-facing ingestion increment.
- **Incremental Compatibility Confirmation**: If changes may alter behavior/contracts/assumptions
  of previously delivered modules, record explicit maintainer confirmation before implementation.
  - **Status**: Pass. The feature consumes the `003-device-discovery` contract without changing it and
    does not alter probe behavior.
- **Verification Path**: Define objective validation for each critical requirement.
  - **Status**: Pass. Unit/contract tests cover mapping, validation, idempotency identity, retry
    behavior, and fake receiver forwarding; gated integration tests cover Kafka consume/decode from
    `devices.detected`.

## Project Structure

### Documentation (this feature)

```text
specs/004-device-ingestion/
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
│   └── SeedWork/
├── NetworkMonitoring.Probe/
└── NetworkMonitoring.IntegrationConsole/
    ├── Application/
    │   ├── Configuration/
    │   ├── Models/
    │   ├── Ports/
    │   └── UseCases/
    ├── Infrastructure/
    │   ├── Backend/
    │   ├── Ingestion/
    │   └── Serialization/
    ├── Host/
    │   ├── DependencyInjection/
    │   └── Services/
    ├── Dockerfile
    ├── appsettings.json
    └── NetworkMonitoring.IntegrationConsole.csproj

tests/
├── NetworkMonitoring.Probe.UnitTests/
├── NetworkMonitoring.Probe.IntegrationTests/
├── NetworkMonitoring.IntegrationConsole.UnitTests/
└── NetworkMonitoring.IntegrationConsole.IntegrationTests/
```

**Structure Decision**: Add one separate deployable worker project for the Integration Console and
separate test projects for its unit/integration coverage. This is proportional because the component
has its own runtime, configuration, Kafka consumer, HTTP adapter, and container packaging.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|

## Post-Design Constitution Check

- **Shared Domain Integrity**: Pass. Design reuses existing shared value objects only; no shared
  entity changes are introduced.
- **SeedWork Immutability**: Pass. No SeedWork edits are required.
- **Boundary Contracts**: Pass. `DeviceDetected` remains unchanged; new `POST /devices` intake
  contract is documented in `contracts/device-intake-http.md`.
- **Security Controls**: Pass. Configuration includes Kafka/Schema Registry security and backend
  transport settings; user authentication/RBAC remains out of scope for this non-user-facing worker.
- **Incremental Compatibility Confirmation**: Pass. No prior module behavior is changed.
- **Verification Path**: Pass. Contract, unit, fake receiver, and gated Kafka integration paths are
  defined by the design artifacts.
