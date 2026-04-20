# Contract: Probe Output Port

## Purpose
Define the application-layer output contract used by probe use cases to publish detected
entities without coupling to a concrete destination.

## Port Naming
- Canonical interface for this feature: `IMessagePublisher` (an equivalent name such as
  `IEntityPublisher` is acceptable if used consistently).

## Port Interface (Conceptual)
- `PublishSessionDetected(session)`

## Behavioral Contract
- Calls are non-blocking from domain perspective; transport details are delegated to adapter.
- Published payloads must pass entity validation rules before dispatch.
- Failure in one publish operation must not stop processing of subsequent observations.
- Adapters must provide structured diagnostics for failed dispatches.
- Invalid observations are filtered before publication via validation-result handling; exceptions are
  reserved for unexpected runtime failures.

## Current Adapter (This Phase)
- `ConsolePublisher` writes structured records to standard output.

## Future Adapter (Next Phase)
- `KafkaPublisher` publishes equivalent records to event topics while preserving payload shape.

## Compatibility Rule
- Any new output adapter must implement the same port and preserve payload schema for
  `SessionDetected`.

## Implementation Note (Current State)
- Implemented port adapter: `ConsolePublisher` in
  `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsolePublisher.cs`.
- Implemented serializer: `ConsoleRecordSerializer` in
  `src/NetworkMonitoring.Probe/Infrastructure/Publishing/ConsoleRecordSerializer.cs`.
- Use case orchestration: `ProcessObservationsUseCase` in
  `src/NetworkMonitoring.Probe/Application/UseCases/ProcessObservationsUseCase.cs`.
- Serialized entity/value object types come from shared domain in
  `src/NetworkMonitoring.Domain/Shared/Entities/` and
  `src/NetworkMonitoring.Domain/Shared/ValueObjects/`.

## Scope of this contract
- This document defines session publication only. The deployed probe binary may include additional
  types and methods beyond this contract; consumers validating session behavior MUST rely on
  `SessionDetected` payloads as specified here.
