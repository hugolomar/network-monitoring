# Feature Specification: Device Inventory

**Feature Branch**: `005-device-inventory`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Implement the real Devices backend API that receives `POST /devices` requests from the Integration Console, persists devices, enforces idempotency by normalized MAC, and exposes a minimal device query endpoint for the later UI."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Accept Device Intake (Priority: P1)

As a platform operator, I want the Devices backend to accept valid device intake requests from the
Integration Console so discovered devices are stored in the platform instead of stopping at a fake
receiver.

**Why this priority**: This is the primary handoff from `004-device-ingestion`; without accepting and
storing intake requests, no real device backend behavior exists.

**Independent Test**: Submit a valid device intake request directly to the backend and verify that the
device is accepted, normalized, stored, and visible through backend diagnostics without requiring the
probe, Kafka, or UI.

**Acceptance Scenarios**:

1. **Given** a valid device intake request with a matching `Idempotency-Key` and normalized MAC,
   **When** the backend receives the request, **Then** it accepts the device and records the device
   fields needed for future retrieval.
2. **Given** the request includes optional hostname and observed IP evidence, **When** the backend
   stores the device, **Then** those values are preserved according to the device identity.
3. **Given** the request includes first-seen and last-seen timestamps, **When** the backend stores the
   device, **Then** the stored device preserves a valid observation time range.

---

### User Story 2 - Preserve Idempotent Device State (Priority: P1)

As a platform maintainer, I want repeated intake requests for the same normalized MAC to update or
confirm the same stored device rather than creating duplicate device records.

**Why this priority**: Device detections are repeated by design, and the Integration Console may retry
requests. Idempotency by MAC is required before this backend can safely receive production ingestion.

**Independent Test**: Submit the same valid device intake request multiple times with the same
idempotency identity and verify that only one stored device identity exists while diagnostics clearly
show duplicate or idempotent handling.

**Acceptance Scenarios**:

1. **Given** a device already exists for a normalized MAC, **When** the same intake request is submitted
   again with the same idempotency identity, **Then** the backend returns an accepted/idempotent outcome
   without creating a second device.
2. **Given** a later request for the same MAC includes new observation evidence, **When** the backend
   processes it, **Then** the existing device is updated or consolidated rather than duplicated.
3. **Given** a retry arrives after an uncertain delivery outcome, **When** the backend receives the same
   idempotency identity, **Then** the resulting stored device state remains safe and deterministic.

---

### User Story 3 - Reject Invalid Intake Safely (Priority: P1)

As an operator, I want invalid device intake requests rejected with clear reasons so bad ingestion data
does not corrupt the stored device inventory.

**Why this priority**: The backend owns persisted device state. It must reject invalid identity,
timestamp, and evidence data before exposing that state to later features.

**Independent Test**: Submit malformed or identity-ambiguous intake requests directly to the backend
and verify that they are rejected, no device is persisted, and diagnostics explain the reason.

**Acceptance Scenarios**:

1. **Given** the `Idempotency-Key` is missing, blank, or malformed, **When** the backend receives the
   request, **Then** it rejects the request and records an operator-visible validation reason.
2. **Given** the `Idempotency-Key` and body `macAddress` identify different devices, **When** the
   backend receives the request, **Then** it rejects the request without changing stored device state.
3. **Given** the request contains invalid MAC, invalid IP evidence, missing required fields, or a
   last-seen timestamp earlier than first-seen, **When** the backend validates the request, **Then** it
   rejects the request without persisting partial data.

---

### User Story 4 - Query Stored Devices (Priority: P2)

As a future UI consumer, I want to retrieve the stored device inventory so `006-device-ui` can display
devices discovered by the platform.

**Why this priority**: The next feature needs a read surface. A minimal query endpoint keeps this
backend useful beyond intake without implementing presentation concerns.

**Independent Test**: Store one or more devices through intake requests, then request the device list
and verify it returns the persisted device identities and key fields.

**Acceptance Scenarios**:

1. **Given** devices have been accepted by the backend, **When** a client requests the device list,
   **Then** the response includes each stored device once by normalized MAC identity.
2. **Given** no devices have been stored, **When** a client requests the device list, **Then** the
   response is successful and clearly represents an empty inventory.
3. **Given** a device was consolidated through repeated detections, **When** a client requests the
   device list, **Then** the response reflects the current consolidated device state.

---

### User Story 5 - Validate End-to-End Backend Handoff (Priority: P2)

As an operator, I want the Integration Console to forward to the real backend so the ingestion path can
be validated from consumed device events through stored backend state.

**Why this priority**: The backend must prove it satisfies the contract established in `004` before the
fake receiver can be retired from validation flows.

**Independent Test**: Run the Integration Console with its backend target set to the real backend,
submit a valid device event through the ingestion path, and verify that the backend stores exactly one
device for the normalized MAC.

**Acceptance Scenarios**:

1. **Given** the real backend is running and configured as the Integration Console target, **When** the
   Integration Console forwards a valid device intake request, **Then** the backend accepts and stores
   the device.
2. **Given** the Integration Console retries a device intake request, **When** the backend receives
   repeated requests for the same normalized MAC, **Then** stored backend state remains idempotent.

### Edge Cases

- The request body is malformed, empty, or has an unsupported content type.
- The `Idempotency-Key` header is missing, blank, malformed, or does not match body `macAddress`.
- `macAddress`, `primaryIp`, or `observedIps` contain invalid values.
- `lastSeenUtc` is earlier than `firstSeenUtc`.
- A request for an existing MAC includes earlier first-seen evidence or later last-seen evidence.
- A request for an existing MAC includes additional observed IPs or a newer hostname.
- Duplicate requests arrive concurrently for the same normalized MAC.
- The persistence layer is unavailable or fails during intake.
- The device inventory is empty when queried.
- A query is made while intake requests are being processed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Devices backend MUST run independently from the probe and Integration Console.
- **FR-002**: The Devices backend MUST expose a device intake operation compatible with the
  `POST /devices` contract defined by `004-device-ingestion`.
- **FR-003**: The Devices backend MUST require `Idempotency-Key` for device intake requests.
- **FR-004**: The Devices backend MUST verify that `Idempotency-Key` and body `macAddress` identify the
  same normalized MAC before accepting a request.
- **FR-005**: The Devices backend MUST validate required device fields before persisting or updating a
  device.
- **FR-006**: The Devices backend MUST reject invalid MAC, invalid IP evidence, malformed request body,
  missing required fields, and invalid timestamp ordering without changing stored device state.
- **FR-007**: The Devices backend MUST persist accepted device records so they survive process restart
  in the configured local development environment.
- **FR-008**: The Devices backend MUST store one logical device per normalized MAC identity.
- **FR-009**: The Devices backend MUST treat duplicate requests with the same normalized MAC and
  idempotency identity as idempotent, avoiding duplicate device records.
- **FR-010**: The Devices backend MUST consolidate repeated detections for the same normalized MAC into
  the existing device record.
- **FR-011**: The Devices backend MUST preserve or update first-seen and last-seen timestamps according
  to the earliest and latest valid evidence received for the device.
- **FR-012**: The Devices backend MUST preserve observed IP evidence without duplicating identical
  observed IP values for the same device.
- **FR-013**: The Devices backend MUST expose a device inventory query that returns stored devices for
  the later UI feature.
- **FR-014**: The device inventory query MUST return an empty successful result when no devices exist.
- **FR-015**: The Devices backend MUST provide operator-visible diagnostics for accepted, updated,
  duplicate/idempotent, rejected, and persistence-failure outcomes.
- **FR-016**: The Devices backend MUST be verifiable by direct HTTP requests without requiring the
  probe, Kafka, or the Integration Console.
- **FR-017**: The Devices backend MUST be verifiable through the Integration Console forwarding path
  established in `004-device-ingestion`.
- **FR-018**: The Devices backend MUST reuse the shared device domain concepts for normalized MAC, IP
  address, discovery source, and device observation invariants.
- **FR-019**: The feature MUST NOT implement the device UI, login/RBAC, probe-side discovery changes,
  Integration Console Kafka consumption changes, session backend behavior, or a new `DeviceDetected`
  event contract.
- **FR-020**: The feature MUST provide container packaging and operator documentation sufficient to run
  the backend locally with the Integration Console.

### Key Entities *(include if feature involves data)*

- **Device Intake Request**: The inbound request produced by the Integration Console. It includes the
  normalized MAC identity, optional primary IP, optional hostname, observed IP evidence, first/last seen
  timestamps, discovery source, source event metadata, and `Idempotency-Key`.
- **Stored Device**: The persisted device inventory record keyed by normalized MAC. It includes current
  device identity, optional primary IP, optional hostname, observed IP evidence, first/last seen
  timestamps, discovery source, and backend-managed persistence metadata.
- **Idempotency Identity**: The request identity derived from `Idempotency-Key` and normalized MAC. It
  prevents repeated intake attempts from creating duplicate stored devices.
- **Rejected Intake**: A request that cannot safely update device state because it is malformed,
  identity-ambiguous, invalid, or cannot be persisted.
- **Device Inventory Result**: The read representation returned to clients that need to display or
  validate stored devices.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation, 100% of valid device intake requests create or update exactly one stored
  device for the normalized MAC identity.
- **SC-002**: In duplicate-request validation, repeated intake for the same normalized MAC produces zero
  duplicate stored device records.
- **SC-003**: In validation, 100% of malformed, identity-mismatched, or invalid device requests are
  rejected without changing stored device state.
- **SC-004**: After storing devices, inventory queries return the accepted device identities and key
  fields with no duplicate MAC entries.
- **SC-005**: A local validation run can demonstrate the full path from Integration Console forwarding
  to real backend persistence for at least one valid device.
- **SC-006**: Operators can identify accepted, updated, duplicate/idempotent, rejected, and
  persistence-failure outcomes from diagnostics during validation.

## Assumptions

- `004-device-ingestion` is complete and owns the Integration Console forwarding behavior.
- The `POST /devices` contract from `004-device-ingestion` is the compatibility baseline for this
  backend feature.
- The normalized MAC address is the stable device identity for backend idempotency and storage.
- A local development datastore is sufficient for this increment as long as accepted devices survive a
  backend process restart.
- Authentication and user authorization are deferred to a later feature unless required by deployment
  configuration.
- `006-device-ui` will consume the device inventory query but will not be implemented in this feature.
