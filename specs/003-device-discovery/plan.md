# Implementation Plan: Device Discovery Separation

**Branch**: `003-device-discovery` | **Date**: 2026-04-27 | **Spec**: `/home/hugo/network-monitoring/specs/003-device-discovery/spec.md`  
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/003-device-discovery/spec.md` covering probe-side device discovery, console `DeviceDetected` output, consolidation, and remaining Kafka publication to `devices.detected`.

## Summary

This increment keeps device discovery inside the existing probe boundary. The delivered baseline already
detects devices from observations, validates MAC evidence with `DiscoveryValidationResult`,
consolidates repeated detections through the shared-domain `Device` aggregate, applies
`DeviceDeduplicationWindowMinutes`, and emits structured console `DeviceDetected` records.

The remaining work is to publish the same validated and deduplicated `DeviceDetected` outcomes to Kafka
using Avro + Schema Registry on configurable topic `devices.detected`, with normalized MAC as the
message key. Kafka details stay in Infrastructure behind `IMessagePublisher`; the application use case
continues to publish device detections through the existing port.

## Planned Outcomes

### Delivered Baseline — Device Discovery and Console Visibility

- **Intent**: Operators can validate device discovery locally without downstream systems.
- **Primary pieces**: shared-domain `Device`, `DiscoveryValidationResult`,
  `ProcessObservationsUseCase`, `ConsolePublisher`, `ConsoleRecordSerializer`, and
  `DeviceDeduplicationWindowMinutes`.
- **Verification**: existing unit/integration tests for valid discovery emission, invalid evidence
  continuation, schema-stable console output, and repeated detection consolidation.

### Remaining Work — Device Event Stream Publication

- **Intent**: Publish validated device detections to Kafka for downstream consumers.
- **Primary pieces**: `device-detected-value.avsc`, Schema Registry subject `devices.detected-value`,
  default topic `devices.detected`, normalized-MAC message key, topic bootstrap updates, Kafka device
  publisher, and opt-in Kafka integration test.
- **Verification**: SC-005 and SC-006 through mapper/key tests and
  `RUN_KAFKA_INTEGRATION=1` produce/consume validation against the reference stack.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: .NET Worker host; shared SeedWork domain; tshark CLI; Confluent.Kafka,
Confluent.SchemaRegistry, Confluent.SchemaRegistry.Serdes.Avro, Apache.Avro  
**Storage**: Apache Kafka reference stack (3 brokers, KRaft) + Schema Registry for emitted
`DeviceDetected` events; no additional storage in this increment  
**Testing**: xUnit unit/integration tests; opt-in Kafka integration test when the compose stack is up  
**Target Platform**: Linux host/container with capture capability; Docker Compose for Kafka + Registry validation  
**Project Type**: .NET worker/console probe module with clean/hexagonal layering  
**Performance Goals**: Preserve streaming behavior and avoid blocking valid later observations after invalid discovery evidence or transient publish failures  
**Constraints**: SeedWork immutability; invalid discovery inputs must not stop stream processing;
device event key must be normalized MAC; Kafka topic defaults to `devices.detected`; TLS minimum and
mTLS target posture for non-dev environments; contract evolution must remain compatible unless
explicitly versioned  
**Scale/Scope**: Probe-side device discovery and event publication only; downstream consumers of
`devices.detected` are later increments

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Shared Domain Integrity**: `Device` remains the shared-domain aggregate root inheriting
  `Entity` and implementing `IAggregateRoot`; this plan does not introduce another device model.
- **SeedWork Immutability**: No changes are planned under `src/NetworkMonitoring.Domain/SeedWork`.
- **Boundary Contracts**: Console JSONL `DeviceDetected` and Kafka Avro `DeviceDetected` are stable
  artifacts under `specs/003-device-discovery/contracts/`; Kafka uses topic `devices.detected` and
  subject `devices.detected-value`.
- **Security Controls**: Local reference stack may use documented PLAINTEXT relaxation; integration,
  staging, and production target TLS/mTLS for Kafka and Registry clients per existing probe options
  and ADR 0008 posture.
- **Incremental Compatibility Confirmation**: The delivered console/consolidation baseline is
  preserved. Adding device Kafka publication extends `IMessagePublisher` behavior without changing
  session semantics or the `sessions.detected` contract.
- **Verification Path**: Unit tests for Avro mapping and normalized-MAC key; existing discovery tests
  remain; opt-in Kafka integration test validates produce/consume for `devices.detected` (SC-005,
  SC-006).

**Gate status**: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/003-device-discovery/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── device-detected-schema.md
│   ├── device-detected-avro.md
│   ├── device-detected-value.avsc
│   └── device-discovery-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── NetworkMonitoring.Domain/
│   └── Shared/Entities/Device.cs
└── NetworkMonitoring.Probe/
    ├── Application/
    │   ├── Configuration/ProbeOptions.cs
    │   ├── Models/DiscoveryValidationResult.cs
    │   ├── Ports/IMessagePublisher.cs
    │   └── UseCases/ProcessObservationsUseCase.cs
    └── Infrastructure/
        ├── Publishing/
        │   ├── ConsoleRecordSerializer.cs
        │   ├── KafkaProbeEventPublisher.cs
        │   └── Schemas/
        └── Traffic/

tests/
├── NetworkMonitoring.Probe.UnitTests/
└── NetworkMonitoring.Probe.IntegrationTests/

scripts/
└── bootstrap/kafka-topics-init.sh
```

**Structure Decision**: Continue using the existing probe module and folder-level clean/hexagonal
layering. No new downstream service is introduced for this feature; Kafka publication is an
Infrastructure adapter behind the existing Application port.

## Design Artifacts

- `research.md`: records the delivered discovery decisions and the remaining Kafka/Avro device
  publication decisions.
- `data-model.md`: documents `Device`, `DeviceDetected`, validation, consolidation, and Kafka key
  semantics.
- `contracts/`: documents console JSONL and Kafka Avro contracts for `DeviceDetected`.
- `quickstart.md`: documents local validation, Kafka stack startup, topic provisioning, and opt-in
  stream publication verification.

## Post-Design Constitution Check

- **Shared Domain Integrity**: PASS — `Device` remains in shared domain; no duplicate model is planned.
- **SeedWork Immutability**: PASS — no SeedWork edits are required.
- **Boundary Contracts**: PASS — `devices.detected` / `devices.detected-value` are explicit contract
  artifacts with compatible-evolution guidance.
- **Security Controls**: PASS — dev relaxation and TLS/mTLS target posture are documented.
- **Incremental Compatibility Confirmation**: PASS — existing console behavior and session publication
  remain valid; device publication is additive.
- **Verification Path**: PASS — automated mapper/key tests plus opt-in Kafka integration validation.

## Complexity Tracking

No constitution violations or added complexity exceptions.

## Next Step (Spec Kit)

Run `/speckit.tasks` for `/home/hugo/network-monitoring/specs/003-device-discovery`, preserving
completed tasks T001-T030 and adding remaining tasks for device Kafka publication, contracts,
topic provisioning, tests, and quickstart validation.
