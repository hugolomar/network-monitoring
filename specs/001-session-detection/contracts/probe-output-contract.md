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

## Adapters (US1 + US2)

### Console (US1)
- `ConsolePublisher` writes structured JSON lines to standard output via `ConsoleRecordSerializer`
  (shape per `contracts/console-record-schema.md`).

### Kafka + Avro (US2)
- `KafkaSessionPublisher` (`src/NetworkMonitoring.Probe/Infrastructure/Publishing/KafkaSessionPublisher.cs`)
  implements `IMessagePublisher.PublishSessionDetected` by serializing the same validated `Session` to
  **Avro** (`GenericRecord`) using the embedded schema copy of
  `contracts/session-detected-value.avsc`, **Confluent wire format**, and **Schema Registry**
  (subject **`sessions.detected-value`**, `SubjectNameStrategy.Topic`, default topic
  **`sessions.detected`**). Registration on first produce is enabled for dev (`AutoRegisterSchemas`);
  production should rely on governed registration and explicit topic provisioning.
- **Partition key**: `SessionKafkaPartitionKey.Build(session)` — same five-tuple identity as session
  deduplication in `ProcessObservationsUseCase` (spec FR-014).
- **Mapping**: `SessionDetectedAvroMapper.ToGenericRecord(session, occurredAtUtc)` in
  `SessionDetectedAvroMapper.cs`.
- **Composition**: `CompositeMessagePublisher` fans out to one or more `IMessagePublisher` instances;
  host wiring in `ServiceCollectionExtensions.CreateMessagePublisher` selects console only, Kafka
  only, or both from `ProbeOptions.EnableConsole` / `EnableKafka` (if both flags are off, console is
  forced so the probe remains usable).

`PublishDeviceDetected` is a no-op on `KafkaSessionPublisher` in this feature scope (device stream
is separate work).

## Compatibility Rule
- Any new output adapter must implement the same port and preserve payload schema for
  `SessionDetected`.

## Implementation Note (Current State)
- Implemented port adapters: `ConsolePublisher`, `KafkaSessionPublisher`, and optional
  `CompositeMessagePublisher` under `src/NetworkMonitoring.Probe/Infrastructure/Publishing/`.
- Console serializer: `ConsoleRecordSerializer` in
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
