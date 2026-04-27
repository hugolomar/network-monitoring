# Contract: Device Discovery Output Port

## Purpose
Define the device-specific output contract for discovery events.

## Port Interface (Conceptual)
- `PublishDeviceDetected(device)`

## Behavioral Contract
- Only validated discovery records are emitted.
- Invalid discovery observations are rejected with diagnostics and do not terminate processing.
- Consolidation semantics must produce deterministic device lifecycle updates.
- Output schema remains stable across repeated emissions unless explicitly versioned.
- Optional emission throttling by normalized MAC and `DeviceDeduplicationWindowMinutes` may reduce
  output frequency without skipping domain consolidation (see `spec.md` FR-012).

## Current Adapter Context
- Current operator-visible destination is structured console output.
- Event-stream destination is Kafka topic `devices.detected` with Avro value subject
  `devices.detected-value`.
- Kafka publication must be an Infrastructure adapter behind the same application port; discovery
  use-case boundaries do not change.

## Implementation Note (Current State)
- Use case: `ProcessObservationsUseCase` in
  `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`.
- Publisher adapter: `ConsolePublisher` in
  `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsolePublisher.cs`.
- Serializer: `ConsoleRecordSerializer` in
  `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs`.
- Port interface: `IMessagePublisher` in
  `src/NetworkMonitoring.Probe/Application/Ports/IMessagePublisher.cs` (host may expose additional
  methods for other outputs; this contract governs only `PublishDeviceDetected`).
- Domain: `Device` in `src/NetworkMonitoring.Domain/Shared/Entities/Device.cs`.
