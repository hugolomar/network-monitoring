# Feature Specification: Device Ingestion

**Feature Branch**: `004-device-ingestion`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Build the Integration Console ingestion component that consumes DeviceDetected events from Kafka topic devices.detected and forwards valid device detections to the backend Devices HTTP API using POST /devices."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consume Device Detections (Priority: P1)

As an operator, I want the Integration Console to consume `DeviceDetected` events from
`devices.detected` so device detections can leave the probe boundary and enter the platform ingestion
flow.

**Why this priority**: Without event consumption, no downstream ingestion behavior can be validated.
This is the smallest useful slice after `003-device-discovery`.

**Independent Test**: With a controlled event stream containing one valid `DeviceDetected` event, run
the Integration Console and verify it accepts the event, preserves the normalized MAC identity, and
records successful processing progress without requiring the real backend.

**Acceptance Scenarios**:

1. **Given** a valid `DeviceDetected` event exists on `devices.detected`, **When** the Integration
   Console is running, **Then** it consumes the event and treats the event key and payload MAC as the
   same device correlation identity.
2. **Given** the Integration Console starts with an empty processing position, **When** valid device
   events are available, **Then** it processes them in a controlled, repeatable way and records
   operator-visible progress.

---

### User Story 2 - Forward Valid Devices (Priority: P1)

As a platform operator, I want each valid `DeviceDetected` event forwarded to `POST /devices` so the
future Devices backend can ingest discovered devices through a stable HTTP contract.

**Why this priority**: Forwarding is the core purpose of this Integration Console component and defines
the handoff contract for the later backend feature.

**Independent Test**: Run the Integration Console against a fake HTTP receiver and verify that a valid
`DeviceDetected` event produces exactly one `POST /devices` request with the expected device fields and
normalized MAC identity.

**Acceptance Scenarios**:

1. **Given** a valid `DeviceDetected` event with MAC, observed IPs, timestamps, hostname, and discovery
   source, **When** it is processed, **Then** the Integration Console sends a `POST /devices` request
   containing the expected JSON body.
2. **Given** the backend base URL is configured, **When** a valid event is processed, **Then** the
   request is sent to that configured destination rather than a hard-coded target.
3. **Given** the event key and payload `macAddress` disagree, **When** the event is processed, **Then**
   the Integration Console rejects or quarantines the event and logs a structured diagnostic instead
   of forwarding ambiguous identity.

---

### User Story 3 - Recover From Delivery Failures (Priority: P2)

As an operator, I want transient backend or network failures retried and permanent failures reported
clearly so device ingestion is reliable and diagnosable.

**Why this priority**: Device ingestion depends on external services; operators need predictable retry
behavior and clear failure visibility before this can run unattended.

**Independent Test**: Run the Integration Console against a fake receiver that first returns transient
failures and later succeeds; verify retries happen according to configured limits and the event is not
lost silently.

**Acceptance Scenarios**:

1. **Given** the device receiver temporarily fails, **When** a valid detection is forwarded, **Then** the
   Integration Console retries according to configured retry limits and backoff.
2. **Given** the receiver keeps failing beyond the configured limit, **When** retries are exhausted,
   **Then** the Integration Console records a clear failure with enough context to identify the device
   and the reason.
3. **Given** the receiver returns a permanent validation error, **When** the event is processed, **Then**
   the Integration Console does not retry indefinitely and records the event as rejected.

---

### User Story 4 - Preserve Idempotent Device Intake (Priority: P2)

As a platform maintainer, I want repeated detections for the same normalized MAC handled idempotently
according to the `POST /devices` contract so retries or duplicate events do not create duplicate
devices downstream.

**Why this priority**: Device detections are naturally repetitive; idempotency protects the later
backend and keeps ingestion behavior safe under retry and replay.

**Independent Test**: Run the Integration Console against a fake HTTP receiver with duplicate
`DeviceDetected` events for the same normalized MAC and verify the requests carry the same idempotency
identity and the fake receiver observes no duplicate downstream effect.

**Acceptance Scenarios**:

1. **Given** duplicate `DeviceDetected` events for the same normalized MAC, **When** the events are
   forwarded, **Then** each request carries the same idempotency/correlation identity.
2. **Given** a retry occurs after an uncertain delivery outcome, **When** the Integration Console sends
   the request again, **Then** the request remains safe for a receiver to treat as the same device
   intake attempt.

---

### Edge Cases

- A `DeviceDetected` event cannot be deserialized or does not match the declared event contract.
- The event key is missing, blank, malformed, or differs from the payload `macAddress`.
- Required device fields are missing or invalid after deserialization.
- The backend base URL or required ingestion configuration is missing at startup.
- The receiver returns transient failures, permanent validation failures, or times out.
- Duplicate events arrive for the same normalized MAC through replay, retry, or normal probe
  deduplication windows.
- A poison message repeatedly fails and would otherwise block later valid events.
- The Integration Console is stopped and restarted while events are available.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Integration Console MUST run independently from the probe process.
- **FR-002**: The Integration Console MUST consume `DeviceDetected` events from the configured device
  event stream, defaulting to `devices.detected`.
- **FR-003**: The Integration Console MUST decode the `DeviceDetected` value using the declared
  `devices.detected-value` event contract.
- **FR-004**: The Integration Console MUST use the event key as the normalized MAC correlation identity.
- **FR-005**: The Integration Console MUST verify that the event key and payload `macAddress` identify
  the same normalized MAC before forwarding.
- **FR-006**: The Integration Console MUST validate required `DeviceDetected` fields before forwarding
  a device intake request.
- **FR-007**: The Integration Console MUST map valid `DeviceDetected` fields to a device intake request
  for `POST /devices`.
- **FR-008**: The Integration Console MUST send valid device intake requests to a configurable backend
  base URL.
- **FR-009**: The device intake request MUST preserve normalized MAC as the idempotency and correlation
  identity.
- **FR-010**: The Integration Console MUST retry transient HTTP or network failures according to
  configurable retry limits and backoff.
- **FR-011**: The Integration Console MUST stop retrying permanent validation failures and record them
  as rejected events.
- **FR-012**: The Integration Console MUST provide structured operator-visible diagnostics for
  consumption, forwarding, retry exhaustion, validation rejection, and poison-message handling.
- **FR-013**: The Integration Console MUST avoid crashing on malformed, unreadable, invalid, or poison
  events.
- **FR-014**: The Integration Console MUST expose configuration for event-stream connection settings,
  device topic, consumer group identity, backend base URL, retry policy, and security settings.
- **FR-015**: The feature MUST be verifiable without the real backend by using a fake or test HTTP
  receiver.
- **FR-016**: The feature MUST NOT implement the real Devices backend, database persistence,
  `GET /devices`, UI, login, or RBAC.
- **FR-017**: The feature MUST NOT change probe-side device discovery behavior or the published
  `DeviceDetected` contract from `003-device-discovery`.

### Key Entities *(include if feature involves data)*

- **DeviceDetected Event**: The event emitted by the probe for a validated device detection. Includes
  normalized MAC, optional primary IP, optional hostname, observed IPs, first/last seen timestamps, and
  discovery source.
- **Device Intake Request**: The request sent to the future Devices backend. Carries the device fields
  needed for intake and the normalized MAC identity used for idempotency.
- **Ingestion Attempt**: One attempt to forward a valid event to the receiver, including retry count,
  outcome, and diagnostic context.
- **Rejected Event**: An event that cannot be forwarded because it is malformed, unreadable, invalid,
  identity-ambiguous, or permanently rejected by the receiver.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a controlled validation run, 100% of valid sampled `DeviceDetected` events are
  forwarded as `POST /devices` requests with the expected normalized MAC identity.
- **SC-002**: In validation with duplicate events for the same MAC, duplicate processing produces no
  duplicate downstream device effect in the fake receiver.
- **SC-003**: In validation with transient receiver failures, events are retried according to the
  configured policy and either succeed or produce a clear retry-exhausted diagnostic.
- **SC-004**: In validation with malformed or invalid events, the Integration Console continues
  processing later valid events without crashing.
- **SC-005**: Operators can identify consumption, forwarding success, retry exhaustion, and rejection
  outcomes from structured diagnostics for every sampled failure scenario.

## Assumptions

- `003-device-discovery` is complete and owns probe-side detection plus `DeviceDetected` publication.
- `005-device-inventory` will implement the real device inventory API and persistence; this feature defines and
  exercises the intake contract with a fake or test receiver.
- `006-device-ui` will implement presentation concerns later; no UI behavior is part of this feature.
- The normalized MAC address is the stable device identity for ingestion idempotency.
- Permanent backend validation failures are not retried indefinitely.
- Security settings are configurable in this feature, but full production identity/RBAC behavior is
  handled by later backend/UI features unless explicitly required by deployment configuration.
