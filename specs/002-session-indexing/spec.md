# Feature Specification: Session Indexing

**Feature Branch**: `002-session-indexing`  
**Created**: 2026-04-20  
**Status**: In progress — reference indexing path delivered (Elasticsearch + Kafka Connect) per `tasks.md` / `quickstart.md`  
**Input**: User description: "Provide queryable history for emitted session detections."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Query Past Session Detections (Priority: P1)

As an **analyst or platform operator**, I want to **search and filter** session detections that
**have already been emitted** so I can **investigate, audit, or correlate** behavior **without relying
solely on live capture output**.

**Why this priority**: Live output proves the probe; the event stream feeds the platform;
**searchable access to history** supports operational and investigative use of the **same**
detections.

**Independent Test**: Given session detections that have been emitted to the organization's
**asynchronous event stream** and are **available through the agreed query capability**, when the
operator applies **documented filters**, **then** returned records match the **same session
semantics and required fields** as the published session event contract, and non-matching
detections are excluded.

**Acceptance Scenarios**:

1. **Given** detections exist within a time range, **When** the operator queries with that range,
   **Then** matching detections are returned with required fields present and consistent meaning.
2. **Given** normalized source, destination, ports (when applicable), and protocol, **When** the
   operator applies matching filters, **Then** only detections that satisfy those criteria are
   returned.
3. **Given** no detection satisfies the filters, **When** the operator runs the query, **Then** the
   outcome is an **empty result** as the normal case (not a fault).
4. **Given** a validated detection was emitted to the stream, **When** it has become **available for
   query** (within any **documented** eligibility delay), **Then** it can be found using filters that
   describe it.

---

### Edge Cases

- Queries over very broad criteria could return large result sets; operators need **bounded**
  retrieval per interaction (see FR-003).
- There may be a **delay** between stream emission and query availability; behavior MUST be
  **documented for operators** when it is not immediate (see FR-004).
- Duplicate or overlapping records in query results MUST NOT contradict the **declared contract**;
  any deduplication or consolidation rules MUST be **documented** for consumers of the query
  capability. Stream-side emission semantics remain owned by `001-session-detection`; this edge case
  covers how the query surface may still present overlaps (e.g. projection or replay) without
  conflicting with FR-002.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a **documented query capability** (product- and
  deployment-specific name) through which authorized operators can **retrieve** past session
  detections using **filters** that include at least: a **time range**, and the same **normalized**
  address, port (when applicable), and protocol dimensions as in emitted records from
  `001-session-detection`.
- **FR-002**: Results of the query capability MUST present each session detection with the **same
  semantic meaning** and **required fields** as the **versioned session event contract** under
  `specs/001-session-detection/contracts/`; the query capability MUST NOT introduce **conflicting**
  interpretations of "session" or of required attributes.
- **FR-003**: Each interactive query interaction MUST return a **bounded** set of results (e.g.
  paging, cursors, or explicit limits) so operators receive a **finite** batch unless a **documented**
  operational exception applies.
- **FR-004**: If detections are not **immediately** available for query after stream publication, the
  **maximum acceptable delay** or the fact of **eventual consistency** MUST be **documented** for
  operators setting expectations.
- **FR-005**: Access to the query capability MUST follow **organization-defined** access rules (who
  may see which detections); enforcement details belong to implementation and security architecture,
  not to this specification.

### Key Entities *(include if feature involves data)*

- **Session Index Document**: Query-optimized representation of a published `SessionDetected` event.
  It reflects the same session semantics as the event contract and is not a separate domain entity.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When the query capability is exercised in a controlled environment, 100% of sampled
  **returned** session detections match the declared contract for required fields and semantics.

## Assumptions

- `001-session-detection` publishes validated session detections to the organization's asynchronous
  event stream before indexing scenarios can be satisfied.
- Kafka remains the durable event log for session detections; the indexing store is a query-optimized
  projection.
- The indexing path may be eventually consistent, and operators need documented expectations for
  emission-to-query availability.
- Query authorization follows organization policy and may be implemented by infrastructure, an API
  layer, or managed service controls.
