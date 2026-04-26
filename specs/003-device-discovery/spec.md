# Feature Specification: Device Discovery Separation

**Feature Branch**: `003-device-discovery`  
**Created**: 2026-04-06  
**Status**: Draft  
**Input**: User description: "Independent specification for device discovery with clear scope,
validation, contracts, and evolution notes."

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

### Edge Cases

- Observation contains MAC but no valid IP evidence for device fields.
- Observation contains IP but no valid MAC evidence.
- Same device appears with changing hostname values over time.
- Same device appears with multiple observed IPs in short intervals.
- Out-of-order observation timestamps arrive for the same device identity.
- High-frequency duplicate detections occur during traffic bursts.

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
- **FR-006**: The system MUST define a stable device event contract suitable for console output now
  and Kafka publication in future increments.
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

## Assumptions

- Backend persistence and inventory reconciliation details are handled in later increments.
- Existing shared-domain abstractions remain available and authoritative for discovery modeling.
- The probe host may load a shared `Probe` configuration section; this specification defines behavior
  and keys only for device discovery (including `DeviceDeduplicationWindowMinutes` and any
  discovery-relevant settings referenced herein). Other keys in the same section may exist for
  unrelated features and are not normative here.
- Device emission deduplication state is in-memory per probe process and does not persist across
  restarts; it is orthogonal to aggregate consolidation rules.
