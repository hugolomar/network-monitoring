# Feature Specification: Device Discovery Separation

**Feature Branch**: `003-device-discovery`  
**Created**: 2026-04-06  
**Status**: In progress — console discovery/consolidation delivered; device event stream publication pending  
**Input**: User description: "Independent specification for device discovery with clear scope,
validation, contracts, event-stream publication, and evolution notes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Detect Devices from Observations (Priority: P1)

As an operator validating discovery behavior, I want device detections produced from probe
observations so that asset visibility is confirmed as a first-class discovery outcome.

**Why this priority**: Device visibility is the core value of this slice and must be independently
demonstrable as an MVP.

**Independent Test**: Run the discovery path with observation samples and verify device records are
emitted with required fields, while invalid device inputs are dropped without stopping processing.

**Acceptance Scenarios**:

1. **Given** valid observation data containing MAC/IP evidence, **When** discovery runs,
   **Then** a device detection record is emitted.
2. **Given** invalid or partial device evidence, **When** discovery runs, **Then** the invalid input
   is rejected with diagnostics and later observations continue to be processed.

---

### User Story 2 - Consolidate Repeated Device Detections (Priority: P2)

As an operator, I want repeated detections of the same device consolidated by clear rules so that
device timelines remain meaningful and not duplicated noisily.

**Why this priority**: Discovery quality depends on consistency over time (`first seen`/`last seen`)
and controlled duplicate behavior.

**Independent Test**: Feed repeated detections for the same device identity and verify consolidation
rules are applied consistently.

**Acceptance Scenarios**:

1. **Given** repeated detections for the same device identity, **When** consolidation logic is
   applied, **Then** timeline fields are updated according to defined rules.

---

### User Story 3 - Self-contained device discovery scope (Priority: P3)

As a maintainer, I want device discovery requirements to be fully defined within this specification
set so that implementation and review do not depend on unrelated feature documents.

**Why this priority**: Keeps ownership and change impact localized when the platform grows (for example
Kafka or additional adapters).

**Independent Test**: Using only artifacts under `specs/003-device-discovery/` (including
`contracts/`), a reviewer can state what “done” means for device discovery without importing
requirements from other feature specifications.

**Acceptance Scenarios**:

1. **Given** this specification and its contracts, **When** a change affects device detection,
   validation, consolidation, or `DeviceDetected` output, **Then** the change can be justified and
   reviewed solely against those artifacts.

---

### User Story 4 - Publish Device Detections to Event Stream (Priority: P2)

As a platform operator, I want validated device detections published to the organization's
asynchronous event stream so downstream consumers can process discovered assets without coupling
directly to the probe process.

**Why this priority**: Console output validates discovery locally; event-stream publication is the
platform integration point for device discovery consumers.

**Independent Test**: With device stream publication enabled against a controlled Kafka environment,
consume the configured device destination and verify sampled messages match the declared
`DeviceDetected` contract, use the normalized MAC as the correlation key, and exclude invalid
discovery inputs.

**Acceptance Scenarios**:

1. **Given** device stream publication is enabled and the stream is reachable, **When** the probe emits
   a validated `DeviceDetected` record, **Then** a corresponding event is available on the configured
   destination.
2. **Given** invalid discovery evidence, **When** the probe rejects that evidence, **Then** no device
   event is published for that evidence and later observations continue to be processed.
3. **Given** repeated detections for the same normalized MAC within the configured device
   deduplication window, **When** publication is enabled, **Then** event-stream publication follows the
   same suppression behavior as operator-visible output while in-memory consolidation still runs.

---

### Edge Cases

- Observation contains MAC but no valid IP evidence for device fields.
- Observation contains IP but no valid MAC evidence.
- Same device appears with changing hostname values over time.
- Same device appears with multiple observed IPs in short intervals.
- Out-of-order observation timestamps arrive for the same device identity.
- High-frequency duplicate detections occur during traffic bursts.
- Event stream is temporarily unavailable while device detection continues.
- Device publication is enabled without operator-visible console output.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST process probe observations to derive device discovery events as a
  dedicated flow owned by this specification.
- **FR-002**: The system MUST require valid device identity evidence before emitting a device
  detection record.
- **FR-003**: The system MUST apply explicit validation-result handling for discovery inputs,
  aggregating validation errors and continuing processing after invalid inputs.
- **FR-004**: The system MUST define and apply deterministic consolidation semantics for repeated
  detections of the same device identity.
- **FR-005**: The system MUST preserve device temporal lifecycle semantics, including initial and
  latest observation timestamps.
- **FR-006**: The system MUST define a stable device event contract suitable for operator-visible
  console output and Kafka publication in this increment.
- **FR-007**: The device domain model MUST remain in shared domain and inherit identity from
  `SeedWork.Entity`, with `Device` remaining an aggregate root.
- **FR-008**: Device discovery behavior, validation rules, consolidation semantics, and
  `DeviceDetected` contracts MUST be fully specified in this increment or in linked artifacts under
  `specs/003-device-discovery/` (for example `contracts/`).
- **FR-009**: Requirements that are not part of device discovery MUST NOT be introduced or duplicated
  in this specification; they belong in their own feature specifications.
- **FR-010**: Discovery validation MUST use a structured validation result (e.g.
  `DiscoveryValidationResult` with validity flag and error messages) for expected invalid inputs
  (such as missing or malformed MAC evidence) before any `DeviceDetected` emission.
- **FR-011**: Repeated detections for the same device identity MUST be consolidated deterministically
  in the probe use case using the shared-domain `Device` aggregate. Correlation identity MUST be the
  normalized MAC address. Consolidation state for this increment MUST be held in process memory
  only (no persistence across probe restarts).
- **FR-012**: The probe MUST support configurable suppression of repeated `DeviceDetected` emissions
  for the same normalized MAC address within a sliding time window. That throttle MUST be
  configurable separately from any other probe emission settings that use distinct configuration
  keys. Identity for this throttle MUST be the normalized MAC. The window length MUST be
  configurable (`DeviceDeduplicationWindowMinutes` under `Probe`); zero or negative MUST disable
  emission deduplication so every consolidated state may be published. The default when configuration
  is omitted MUST be 10 minutes. Domain consolidation (timestamps, observed IPs) MUST still run on
  every valid observation even when an emission is suppressed.
- **FR-013**: When device event publication is enabled, the system MUST emit one event per validated
  `DeviceDetected` output, subject to the same device deduplication and consolidation rules, to the
  configured asynchronous event-stream destination.
- **FR-014**: The device event-stream destination name MUST be configurable. When not overridden, it
  MUST default to the platform standard topic name `devices.detected`.
- **FR-015**: Each published device event MUST include a stable correlation key derived from the
  normalized MAC address so downstream consumers can partition and reconcile detections for the same
  device consistently.
- **FR-016**: Published device event payloads MUST conform to the versioned `DeviceDetected` event
  contract shipped with this feature under `specs/003-device-discovery/contracts/`, including rules
  for compatible evolution of that contract.
- **FR-017**: Device stream publication MUST preserve the same validation behavior as
  operator-visible output: invalid discovery inputs MUST NOT produce device events, and expected
  invalid inputs MUST NOT stop later observations from being processed.
- **FR-018**: Communication with the device event stream MUST be encrypted in transit. In
  production-class environments, the probe MUST authenticate to the stream in a manner that satisfies
  organizational security policy. Non-production environments MAY use relaxed controls only when
  explicitly documented as such.

### Key Entities *(include if feature involves data)*

- **Device**: Represents an observed network asset with shared-domain identity, lifecycle timestamps,
  address evidence, and discovery source metadata.
- **Device Detection Record**: Contracted event payload representing one validated discovery output
  for downstream consumers.
- **Discovery Validation Result**: Structured validation outcome indicating whether observation input
  can produce a valid discovery record and why it may be rejected.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a controlled validation run, 100% of emitted discovery records include required
  contract fields and consistent schema shape.
- **SC-002**: During a 15-minute mixed-input run, processing continues without manual restart after
  invalid discovery inputs.
- **SC-003**: For repeated detections of the same device identity, consolidation behavior is
  consistent across 100% of sampled cases in the validation dataset.
- **SC-004**: Maintainers can state device discovery scope and obligations from this specification
  alone in under 5 minutes.
- **SC-005**: When device stream publication is enabled in a controlled run, 100% of sampled events on
  the configured destination (defaulting to `devices.detected`) match the declared `DeviceDetected`
  contract for required fields and semantics.
- **SC-006**: In the same controlled run, 100% of sampled device event keys match the normalized MAC
  address in the corresponding payload.

## Assumptions

- Downstream consumers of `devices.detected` are handled in later increments.
- Existing shared-domain abstractions remain available and authoritative for discovery modeling.
- Kafka is the target asynchronous event stream for this increment's reference implementation.
- The probe host may load a shared `Probe` configuration section; this specification defines behavior
  and keys only for device discovery (including `DeviceDeduplicationWindowMinutes` and any
  discovery-relevant settings referenced herein). Other keys in the same section may exist for
  unrelated features and are not normative here.
- Device emission deduplication state is in-memory per probe process and does not persist across
  restarts; it is orthogonal to aggregate consolidation rules.
