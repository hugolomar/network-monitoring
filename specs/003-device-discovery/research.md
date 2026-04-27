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
- **Rationale**: Preserves incremental delivery and avoids coupling this spec to downstream storage or
  consumer responsibilities.
- **Alternatives considered**:
  - Move consolidation to a downstream consumer now: potentially useful later, but outside this
    feature scope.

## Decision 3: Consolidation semantics in this phase
- **Decision**: Define deterministic discovery consolidation behavior for repeated detections:
  initial detection sets baseline fields, subsequent detections update latest-seen semantics and
  extend observed evidence.
- **Rationale**: Ensures consistent operator-visible behavior and future compatibility for consumers
  of `devices.detected`.
- **Alternatives considered**:
  - Emit every detection as fully independent without consolidation semantics: simpler but noisy and
    less useful for lifecycle interpretation.

## Decision 4: Contract strategy (console + Kafka in this increment)
- **Decision**: Keep structured device detection payload contracts stable and explicit in this spec,
  with field-level schema suitable for current console emission and Kafka publication to
  `devices.detected`.
- **Rationale**: Supports contract-first boundaries and lets downstream consumers rely on the same
  `DeviceDetected` semantics as the operator-visible output.
- **Alternatives considered**:
  - Delay explicit schema until a later consumer step: postpones clarity and weakens compatibility
    checks.

## Decision 5: Validation approach for discovery inputs
- **Decision**: Use explicit validation pattern for expected invalid discovery inputs (aggregate
  errors + continue stream), reserving exceptions for unexpected runtime failures.
- **Rationale**: Improves resilience and maintainability in high-noise observation environments.
- **Alternatives considered**:
  - Exception-driven validation path: simpler initially but weaker operational behavior.

## Decision 6: Device event value encoding
- **Decision**: Kafka values for `DeviceDetected` use Avro with Schema Registry. The canonical schema
  lives at `specs/003-device-discovery/contracts/device-detected-value.avsc`; the default topic is
  `devices.detected`; the default Registry subject is `devices.detected-value`.
- **Rationale**: This mirrors the delivered `SessionDetected` Kafka pattern, keeps event evolution
  governable, and avoids introducing a separate serialization strategy for device telemetry.
- **Alternatives considered**:
  - JSON on the Kafka wire: easier to inspect but weaker for compatibility enforcement and different
    from the established session publication path.
  - Protobuf: viable, but would add another schema toolchain without a current requirement.

## Decision 7: Device event partition/correlation key
- **Decision**: Use the normalized MAC address as the Kafka record key for `DeviceDetected`.
- **Rationale**: The spec defines normalized MAC as discovery correlation identity and deduplication
  identity, so it is the natural partition key for downstream consumers.
- **Alternatives considered**:
  - Null key: simpler but gives up partition locality for a single device.
  - Composite MAC/IP key: over-specifies identity and conflicts with MAC-only correlation semantics.

## Decision 8: Publisher integration
- **Decision**: Extend the existing probe Infrastructure Kafka publisher behavior for devices while
  keeping publication behind `IMessagePublisher.PublishDeviceDetected`.
- **Rationale**: Preserves hexagonal boundaries and the already delivered use-case flow; Kafka details
  remain outside Application.
- **Alternatives considered**:
  - New device-specific use case: duplicates discovery orchestration.
  - Direct Kafka calls from `ProcessObservationsUseCase`: violates dependency direction.

## Execution Notes
- Plan aligns with constitution articles on shared-domain authority, SeedWork immutability,
  contract-first boundaries, and incremental construction with explicit maintainer confirmation for
  potential prior-module impact.

## Implementation Validation (2026-04-19)
- `dotnet test src/NetworkMonitoring.sln` passed:
  - `NetworkMonitoring.Probe.UnitTests`: 24 passed, 0 failed.
  - `NetworkMonitoring.Probe.IntegrationTests`: 2 passed, 0 failed, 1 Kafka integration test skipped
    unless `RUN_KAFKA_INTEGRATION=1`.
- Discovery validation behavior verified with invalid MAC evidence tests (invalid discovery inputs
  are skipped and stream processing continues).
- Consolidation behavior verified for repeated device detections:
  - deterministic `FirstSeenUtc`/`LastSeenUtc` updates,
  - unique observed IP enrichment,
  - schema-stable serialized `DeviceDetected` payload fields.

## Implementation Validation (2026-04-27)
- `dotnet test src/NetworkMonitoring.sln` passed:
  - `NetworkMonitoring.Probe.UnitTests`: 34 passed, 0 failed.
  - `NetworkMonitoring.Probe.IntegrationTests`: 2 passed, 0 failed, 2 Kafka integration tests skipped
    unless `RUN_KAFKA_INTEGRATION=1`.
- Unit coverage added for `DeviceDetectedAvroMapper`, normalized-MAC Kafka keys, disabled Kafka
  device publication, Kafka-only device publication when console output is disabled, and publish
  failure handling.
- Gated SC-005/SC-006 validation is implemented in
  `KafkaDeviceEventPublishIntegrationTests.PublishDeviceDetected_ProducesConsumableAvroValueWithNormalizedMacKey`.
  It remains skipped by default because the reference Kafka stack was not run for this validation
  pass; execute it with `RUN_KAFKA_INTEGRATION=1` after starting the reference stack and running
  `./scripts/bootstrap/kafka-topics-init.sh`.
