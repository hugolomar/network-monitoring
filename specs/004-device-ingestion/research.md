# Research: Device Ingestion

## Decision 1: Separate Integration Console deployable

- **Decision**: Implement device ingestion as a separate `NetworkMonitoring.IntegrationConsole`
  worker/console process, independent from `NetworkMonitoring.Probe`.
- **Rationale**: The component has a different runtime responsibility: consume platform events and
  forward them to a backend intake contract. Keeping it separate preserves the probe boundary and
  allows independent deployment, configuration, and scaling.
- **Alternatives considered**:
  - Extend the probe to call `POST /devices`: rejected because it couples passive capture to backend
    availability and violates the agreed 003/004 boundary.
  - Wait for backend implementation before ingestion: rejected because 004 can define and validate
    the integration contract using a fake receiver.

## Decision 2: Hexagonal ingestion use case

- **Decision**: Keep the ingestion orchestration in Application behind ports for event consumption and
  device intake forwarding. Kafka, Schema Registry, HTTP, retries, and logging adapters stay in
  Infrastructure.
- **Rationale**: This mirrors existing probe structure and keeps the use case testable without Kafka or
  a real HTTP backend.
- **Alternatives considered**:
  - Direct Kafka and HTTP calls inside the worker service: simpler initially but harder to unit test
    and contrary to repository boundaries.

## Decision 3: Preserve `DeviceDetected` event contract

- **Decision**: Consume the existing `DeviceDetected` Avro event from `devices.detected` using the
  `devices.detected-value` subject and treat the Kafka message key as normalized MAC identity.
- **Rationale**: `003-device-discovery` already owns and validates this contract; ingestion must be a
  downstream consumer, not a contract redefinition point.
- **Alternatives considered**:
  - Introduce an ingestion-specific event schema: rejected because it duplicates the producer contract
    and creates drift.

## Decision 4: Proposed `POST /devices` intake contract

- **Decision**: Define the 004 output contract as `POST /devices` with JSON body fields mapped from
  `DeviceDetected` and an `Idempotency-Key` header equal to the normalized MAC address.
- **Rationale**: The real backend is deferred to 005, but 004 needs a stable forwarding target to
  validate behavior. A request body plus idempotency header is simple, explicit, and easy for the later
  backend to honor.
- **Alternatives considered**:
  - Put idempotency only in the body: rejected because retries are easier to reason about when the
    HTTP request carries a standard idempotency identity.
  - Use a generated event id: rejected because the stable device identity for this increment is the
    normalized MAC.

## Decision 5: Retry and failure classification

- **Decision**: Treat network failures, timeouts, 408, 429, and 5xx responses as transient and
  retryable. Treat malformed events, MAC identity mismatches, and 4xx validation responses as terminal
  rejections, except future 409/duplicate responses may be classified as idempotent success if the
  backend contract defines that behavior.
- **Rationale**: This avoids infinite retries for bad input while preserving resilience for temporary
  backend/service outages.
- **Alternatives considered**:
  - Retry every failure: rejected because poison messages could block ingestion.
  - Drop all failures after one attempt: rejected because transient backend outages are expected.

## Decision 6: Fake receiver for validation

- **Decision**: Validate `POST /devices` forwarding, idempotency, retry, and rejection behavior with a
  fake/test HTTP receiver in 004.
- **Rationale**: The real backend/API and persistence are explicitly out of scope until
  `005-device-inventory`, but the ingestion contract must still be objectively testable.
- **Alternatives considered**:
  - Implement a minimal real backend now: rejected as scope creep into 005.
  - Only document the HTTP contract: rejected because SC-001 through SC-005 require executable
    validation.

## Decision 7: Containerized deployable

- **Decision**: Include container packaging for the Integration Console as part of the implementation
  plan.
- **Rationale**: The constitution requires each deployable unit to maintain a Docker image definition.
  The Integration Console is a separate deployable unit.
- **Alternatives considered**:
  - Delay containerization: rejected because it would leave the deployable incomplete against the
    constitution.

## SeedWork guardrail

The Integration Console references the shared domain project for consistency with the repository's
bounded model, but this feature does not modify SeedWork/domain abstractions. Device ingestion treats
`DeviceDetected` as an integration event and keeps Kafka, HTTP, retry, and logging concerns behind
Application ports.

## Implementation validation

- `dotnet test src/NetworkMonitoring.sln` passed with the Integration Console projects included.
  Result: 62 passed, 3 skipped. Skipped tests are the Kafka-gated integration tests.
- `RUN_KAFKA_INTEGRATION=1` validation was not run because the reference Kafka stack was not requested
  as an active prerequisite for this run. The gated test
  `KafkaDeviceIngestionIntegrationTests.Reference_stack_exposes_devices_detected_topic_and_schema_registry`
  remains skipped unless `RUN_KAFKA_INTEGRATION=1` is set and the reference stack is running.
