# Feature Specification: Device Management

**Feature Branch**: `006-device-management`  
**Created**: 2026-04-28  
**Status**: Draft  
**Input**: User description: "Implement device management as a separate web UI deployable for operators. The UI should consume the existing Device Inventory backend from 005-device-inventory, list discovered and stored devices via GET /devices, allow manual device creation through the existing POST /devices contract, and provide clear loading, empty, validation-error, idempotent-success, and backend-unavailable states. The UI must stay independent from the probe, Integration Console, Kafka, PostgreSQL, and Elasticsearch internals, and must not change existing backend, Kafka, or persistence contracts. It should be containerizable and runnable both inside the local reference stack and as a local development process pointed at the backend container. Authentication, Keycloak/RBAC, mTLS, session search, dashboards, and backend contract changes are out of scope for this increment and should be explicitly deferred."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Device Inventory (Priority: P1)

As a network operator, I want a browser-based device inventory view so I can see discovered and stored
devices without using command-line tools.

**Why this priority**: This is the first user-facing slice over the Device Inventory backend. It proves
operators can validate the persisted inventory through the application rather than through backend-only
checks.

**Independent Test**: Start the backend with at least one stored device, open the UI, and verify the
device list renders the inventory without requiring the probe, Kafka, or Integration Console during the
test.

**Acceptance Scenarios**:

1. **Given** the backend has stored devices, **When** the operator opens the device management UI,
   **Then** the UI displays each device once with normalized MAC, primary IP when present, hostname when
   present, observed IPs, discovery source, and first/last seen timestamps.
2. **Given** the backend inventory is empty, **When** the operator opens the UI, **Then** the UI shows a
   clear empty state instead of an error or blank screen.
3. **Given** the backend is temporarily unavailable, **When** the operator opens the UI, **Then** the UI
   shows a recoverable unavailable state and offers a retry path.

---

### User Story 2 - Refresh Device Inventory (Priority: P1)

As an operator validating the discovery pipeline, I want to refresh the device list so newly stored
devices become visible without restarting the UI.

**Why this priority**: Device inventory changes as automatic discovery and manual creation add or update
devices. A refresh action gives a deterministic validation path before adding live updates.

**Independent Test**: Open the UI against an empty or known inventory, add a device through the existing
backend API or ingestion path, refresh the UI, and verify the list reflects the updated inventory.

**Acceptance Scenarios**:

1. **Given** the UI is already open, **When** a new device is stored by the backend and the operator
   refreshes the list, **Then** the new or updated device appears without a full browser reload.
2. **Given** the refresh request fails, **When** the operator refreshes the list, **Then** the previous
   inventory remains visible and the UI reports that the latest refresh failed.

---

### User Story 3 - Create Device Manually (Priority: P2)

As an operator, I want to create a known device manually so the inventory can include assets before or
without automatic discovery.

**Why this priority**: The architecture requires automatic and manual device creation to pass through
the same backend validation and idempotency path. Manual creation makes the device management capability
useful beyond passive discovery.

**Independent Test**: Open the manual creation form, submit a valid device, verify the backend accepts
it, and verify the inventory list shows the resulting device once.

**Acceptance Scenarios**:

1. **Given** the operator enters valid device details, **When** the form is submitted, **Then** the UI
   sends the request through the existing backend device creation contract and shows a successful result.
2. **Given** the backend rejects submitted device details, **When** the form submission completes, **Then**
   the UI displays the validation reason in operator-readable language and does not present the device
   as created.
3. **Given** the submitted MAC already exists, **When** the backend returns an idempotent or consolidated
   success, **Then** the UI presents the outcome as successful and displays only one inventory row for
   that device identity.

---

### User Story 4 - Run the UI Independently (Priority: P2)

As a developer or operator, I want the device management UI to run as a separate deployable so it can be
started, stopped, configured, and debugged independently from the backend and ingestion services.

**Why this priority**: The architecture treats the web application as a separate component. Independent
deployment keeps the UI from coupling to probe, ingestion, messaging, or persistence internals.

**Independent Test**: Start the local reference stack with the backend and UI, open the documented UI
URL, and verify it can read devices from the backend. Then run the UI as a local development process
pointed at the backend container and verify the same behavior.

**Acceptance Scenarios**:

1. **Given** the local reference stack is running, **When** the UI deployable starts, **Then** it serves
   the device management view on a documented local address.
2. **Given** a developer runs the UI outside the stack, **When** it is configured with the backend base
   URL, **Then** it can use the backend container without changes to backend code or contracts.

### Edge Cases

- Backend returns an empty inventory.
- Backend is unreachable or returns an unavailable response.
- Backend rejects manual creation because of invalid MAC, invalid IP, invalid timestamps, missing
  fields, or idempotency mismatch.
- Device data has no hostname or no primary IP.
- Device data has multiple observed IPs or consolidated timestamps.
- Repeated form submission for the same MAC must not present duplicate inventory rows.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a browser-accessible device management UI for operators.
- **FR-002**: The UI MUST be a separate deployable from the backend, probe, Integration Console, and
  infrastructure services.
- **FR-003**: The UI MUST list devices from the existing Device Inventory backend inventory endpoint.
- **FR-004**: The UI MUST display each returned device's normalized MAC, primary IP when present,
  hostname when present, observed IPs, discovery source, and first/last seen timestamps.
- **FR-005**: The UI MUST show distinct loading, empty, validation-error, idempotent-success, refresh
  failure, and backend-unavailable states.
- **FR-006**: Operators MUST be able to refresh the device inventory without restarting the UI.
- **FR-007**: Operators MUST be able to submit a manual device creation request through the existing
  backend device creation contract.
- **FR-008**: Manual creation MUST rely on backend validation, consolidation, and idempotency as the
  source of truth.
- **FR-009**: The UI MUST NOT connect directly to Kafka, PostgreSQL, Elasticsearch, probe internals, or
  Integration Console internals.
- **FR-010**: The UI MUST be configurable to use the backend exposed by the local reference stack or a
  locally running backend.
- **FR-011**: The feature MUST document how to run the UI inside the local reference stack and how to run
  it as a local development process pointed at the backend container.
- **FR-012**: This increment MUST NOT introduce authentication, Keycloak/RBAC, mTLS, session search,
  dashboards, backend contract changes, Kafka contract changes, or persistence changes.

### Key Entities

- **Device Inventory Item**: A displayed device record from the backend inventory, including identity,
  discovery evidence, and observation timestamps.
- **Manual Device Draft**: Operator-entered device data submitted to the backend for creation or
  consolidation.
- **Device Management State**: UI state representing loading, loaded inventory, empty inventory,
  successful creation/consolidation, validation failure, refresh failure, and backend unavailability.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An operator can start the documented local backend/UI path and view a seeded non-empty
  device inventory in under 5 minutes.
- **SC-002**: With no stored devices, the UI displays an empty inventory state with no runtime errors
  during the documented validation flow.
- **SC-003**: A valid manually created device appears in the inventory after creation or refresh, and a
  repeated submission for the same MAC results in one displayed device.
- **SC-004**: When the backend is unavailable, the UI shows a recoverable error state and succeeds after
  the backend is restored and the operator retries.
- **SC-005**: The UI can be run both as part of the local reference stack and as a local development
  process pointed at the backend container.

## Assumptions

- The Device Inventory backend from `005-device-inventory` remains the authoritative source for device
  validation, persistence, and idempotency.
- Authentication and RBAC are intentionally deferred to a later Keycloak-focused feature.
- mTLS and production service security are intentionally deferred to a later security-focused feature.
- Session search and dashboards are separate future capabilities.
- The UI does not own device persistence and does not define new backend or Kafka contracts.
