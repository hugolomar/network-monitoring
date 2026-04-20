# Feature Specification: Probe Session Detection Visibility

**Feature Branch**: `001-session-detection`  
**Created**: 2026-04-03  
**Status**: In progress — US1 (operator-visible output) delivered; US2 (Kafka / SC-005) in `tasks.md` Phases 5–7  
**Input**: User description: "First probe increment to validate session capture and visibility."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Observe captured sessions live (Priority: P1)

As an operator validating probe behavior, I want to see captured session entities as they are
produced so I can confirm session detection is working.

**Why this priority**: This is the minimum proof that the probe captures and structures session
observations usefully.

**Independent Test**: Start the probe in a monitored environment and verify session records appear
in the output stream when traffic is present.

**Acceptance Scenarios**:

1. **Given** the probe is running and network traffic exists, **When** capture begins, **Then**
   session entities are shown in the output stream.

---

### User Story 2 - Feed the platform session event stream (Priority: P2)

As a platform operator, I want validated session detections to appear on the **organization’s
asynchronous event stream** so downstream services can consume them without being tied to the probe
process.

**Why this priority**: The console proves the probe; the event stream is how the wider platform
integrates and scales.

**Independent Test**: With stream publication enabled against a test environment, consume the
configured session destination and verify each message matches the agreed session event contract for
required fields and meaning.

**Acceptance Scenarios**:

1. **Given** publication is enabled and the stream is reachable, **When** the probe emits a validated
   session detection, **Then** a corresponding event is available on the configured destination.
2. **Given** invalid traffic observations, **When** they are rejected, **Then** no session event is
   published for those observations and processing continues.

---

### Edge Cases

- No traffic is present during capture and therefore no session entities are produced.
- Partial or malformed packet observations occur and cannot form valid session entities.
- Capture starts or stops during active traffic bursts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST detect session observations from monitored network traffic and produce
  session entities suitable for monitoring.
- **FR-002**: The system MUST provide an operator-visible output mode (e.g. console) for emitted
  session detections so behavior can be validated without downstream systems.
- **FR-003**: The system MUST preserve a **stable session detection record shape** across repeated
  emissions (same logical meaning → same required fields and semantics).
- **FR-004**: The system MUST reject or skip observations that cannot satisfy required session fields,
  MUST continue processing subsequent observations, and MUST surface enough detail for operators to
  understand what was dropped.
- **FR-005**: Session identifiers in emitted records MUST follow the platform rule: **no probe-
  assigned surrogate key** until persistent storage assigns one; until then the identifier is absent
  or explicitly represented as unknown per the published contract.
- **FR-006**: Source and destination addresses, ports (when applicable), and protocol MUST appear in
  emitted records in a **normalized, comparable** form (consistent formatting and interpretation).
- **FR-007**: The probe MUST be delivered as **one cohesive deployable service** (single operational
  unit), not a set of loosely coordinated binaries operators must assemble by hand.
- **FR-008**: The meaning of “session” and its attributes in emitted records MUST stay **aligned**
  with the shared platform definition so other components can rely on the same semantics without
  ad-hoc reinterpretation.
- **FR-009**: The probe MUST be **runnable as a container** so the same behavior can be reproduced
  across environments with predictable packaging.
- **FR-010**: For expected invalid input, the system MUST **avoid using failure mechanisms reserved
  for unexpected faults** as the normal way to skip bad observations (behavior stays predictable and
  stream-oriented).
- **FR-011**: The system MUST support **configurable suppression** of repeated session detection
  emissions for the **same logical session** within a sliding time window, so operators are not
  flooded on long-lived flows. Session identity for this purpose MUST be derived deterministically
  from source address, destination address, source port, destination port, and protocol when those
  elements are available for a valid observation. The window MUST be configurable; zero or negative
  MUST disable suppression. If operators do not set a window, the default MUST be **10 minutes**.
- **FR-012**: Operators MUST be able to configure, in one place, at least: **where** capture runs
  (interface), **how** capture is performed (tool path), an **optional** traffic filter, and the
  **duplicate-suppression window** for session emissions.

### Event stream publication *(session detections; next delivery phase)*

These requirements implement **User Story 2**. They add **asynchronous publication** of the same
validated session outcomes as the operator-visible mode, without replacing that mode unless a
deployment turns it off.

- **FR-013**: When publication is enabled, the system MUST emit **one event per validated session
  detection** (subject to the same duplicate-suppression rules as the operator-visible stream) to the
  organization’s **asynchronous event stream**. The **destination name** MUST be configurable; when
  not overridden, it MUST default to the **platform standard** name for session detections.
- **FR-014**: Each published event MUST include a **stable correlation key** for the logical session,
  derived from the same elements used for duplicate suppression, so downstream processing can treat
  one conversation consistently over time.
- **FR-015**: Published event payloads MUST conform to the **versioned session event contract**
  shipped with this feature under `specs/001-session-detection/contracts/`, including rules for
  **compatible evolution** of that contract. Low-level encoding and schema registry mechanics are
  defined in implementation artifacts and **Architecture Decision Records**, not restated here.
- **FR-016**: Communication with the event stream MUST be **encrypted in transit**. In **production-
  class** environments, the probe MUST **authenticate** to the stream in a manner that satisfies
  organizational security policy. **Non-production** environments MAY use relaxed controls only when
  explicitly documented as such.

### Key Entities *(include if feature involves data)*

- **Session**: Represents a network communication observation with source, destination, protocol,
  timing, and traffic-size context required for monitoring.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a controlled validation run with representative traffic, at least one session
  detection is visible in operator-visible output.
- **SC-002**: For repeated observations of the same traffic pattern, 100% of sampled emissions
  share the same required field structure and semantics.
- **SC-003**: During a 15-minute run with mixed valid and invalid observations, session emission
  continues after invalid observations **without** manual restart.
- **SC-004**: Validation stakeholders can confirm probe session capture behavior from operator-
  visible output in under 10 minutes.
- **SC-005**: When stream publication is enabled in a controlled run, 100% of sampled session events
  on the **configured destination** (defaulting to the platform standard name) match the declared
  contract for required fields and semantics.

## Assumptions

- Delivery is **phased**: early focus on **operator-visible** validation; **event stream publication**
  follows in the same specification set but may ship after the first slice.
- This feature is one incremental slice of a larger network monitoring system and does not define
  global architecture for all future modules.
- A testable network segment or equivalent traffic source is available for validation.
- A shared platform definition of “session” exists or is introduced alongside this work so consumers
  agree on meaning.
- Duplicate-suppression state for emission lives **in the running probe** only; it is not a
  substitute for authoritative session storage and does not persist across restarts.
