# Feature Specification: Device Communication Graph

**Feature Branch**: `007-device-communication-graph`  
**Created**: 2026-05-21  
**Status**: In progress (expanded baseline includes graph visualization UI)  
**Input**: User description: "007-device-communication-graph"

## Clarifications

### Session 2026-05-21

- Q: ¿Qué alcance de autorización aplicamos al endpoint de grafo? → A: Acceso autenticado con rol permitido (`admin`, `analyst`, `auditor`, `integration`).
- Q: ¿Permitimos ejecución manual on-demand además del ciclo diario? → A: No, solo ejecución programada cada 24h.
- Q: ¿Qué política aplicamos ante fallo temporal de escritura en proyección? → A: Reintentos acotados; si se agotan, registrar fallo operativo y continuar con siguientes eventos.
- Q: ¿Qué observabilidad mínima hacemos obligatoria en esta feature? → A: Logs estructurados + métricas de contadores/latencia para proyección y retención.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build Communication Links from Session Traffic (Priority: P1)

As a platform operator, I want enriched session traffic to be projected into a device communication
graph so communication relationships can be explored without reading raw traffic streams.

**Why this priority**: This is the core value of the feature; without graph projection there is no
communication topology to inspect.

**Independent Test**: Submit representative enriched session traffic and verify that communication
relationships appear quickly as graph nodes/edges with stable identity and timestamps.

**Acceptance Scenarios**:

1. **Given** valid enriched traffic between two known internal devices, **When** projection runs,
   **Then** one communication relationship exists between those device identities and reflects first/last
   observed times plus cumulative interaction count.
2. **Given** valid enriched traffic from an internal device to an unresolved destination identity,
   **When** projection runs, **Then** the destination is represented as an external host and linked from
   the source device.
3. **Given** duplicated delivery of the same enriched event, **When** both deliveries are processed,
   **Then** graph structure is not duplicated and only relationship counters/timestamps are advanced per
   identity rules.
4. **Given** a projection write fails transiently, **When** bounded retries are exhausted, **Then** the
   failure is emitted as operational diagnostics and processing continues with subsequent events without
   stopping the consumer flow.

---

### User Story 2 - Explore a Device-Centered Subgraph (Priority: P1)

As a viewer, I want to request a communication neighborhood for a selected device so I can visualize
how it interacts with nearby nodes.

**Why this priority**: Users need a navigable graph result, not only background projection.

**Independent Test**: Request graph data for a known root device with varying depth/size inputs and
verify the response includes expected nodes/edges plus truncation signal when limits are reached.

**Acceptance Scenarios**:

1. **Given** a valid root device and one-hop depth, **When** graph data is requested, **Then** the
   response includes the root, directly connected nodes, and edges among returned nodes.
2. **Given** a depth input above the permitted maximum, **When** graph data is requested, **Then** the
   service applies the configured depth cap and returns results using the capped value.
3. **Given** a neighborhood larger than the allowed result size, **When** graph data is requested,
   **Then** the response marks truncation and returns no more than the configured node limit.
4. **Given** both requested depth and requested size exceed configured bounds in the same request,
   **When** graph data is requested, **Then** depth is capped, size is capped, and the response
   explicitly sets truncation when the size cap is hit.

---

### User Story 3 - Enforce Graph Retention Hygiene (Priority: P2)

As a platform maintainer, I want stale communication links and orphan external host nodes pruned on a
scheduled cadence so the graph remains relevant and manageable over time.

**Why this priority**: The graph should represent recent communication behavior and avoid unbounded
growth from inactive relationships.

**Independent Test**: Seed graph data with old communication links and verify scheduled retention removes
out-of-window links and cleans external hosts left without inbound links.

**Acceptance Scenarios**:

1. **Given** a communication relationship whose last activity is older than the retention window,
   **When** the retention run executes, **Then** that relationship is removed.
2. **Given** an external host left without inbound communication links after pruning, **When** cleanup
   continues, **Then** that external host node is removed.
3. **Given** an internal device with no remaining communication links, **When** retention runs, **Then**
   the internal device identity remains preserved by authoritative inventory ownership.
4. **Given** a relationship with `lastSeen` exactly equal to `now - 90 days`, **When** retention runs,
   **Then** the relationship is retained (strictly older-than is removed).

---

### User Story 4 - Graceful Degradation on Graph Store Outage (Priority: P2)

As an operator, I want graph retrieval to fail cleanly when the graph store is unavailable while other
device/session capabilities continue to operate.

**Why this priority**: Graph failure must not cascade into unrelated platform capabilities.

**Independent Test**: Simulate graph store unavailability and verify graph retrieval returns a clear
service-unavailable error while non-graph device/session operations remain healthy.

**Acceptance Scenarios**:

1. **Given** graph storage is unavailable, **When** graph data is requested, **Then** the caller receives
   a service-unavailable response with meaningful error details.
2. **Given** graph storage is unavailable, **When** non-graph device and session capabilities are used,
   **Then** those capabilities continue to return normal successful responses.
3. **Given** graph storage recovers after a transient outage, **When** graph data is requested again,
   **Then** the endpoint returns normal successful responses without requiring service restart.

---

### User Story 5 - Visualize Graph Results in the UI (Priority: P1)

As an operator, I want a dedicated UI view for communication graph exploration so I can inspect nodes,
edges, and truncation outcomes without manually calling backend endpoints.

**Why this priority**: The endpoint by itself is not sufficient for day-to-day operational use; users
need a first-class visualization workflow in the existing frontend.

**Independent Test**: Open the graph UI, load the full snapshot (without root), run root-filtered queries,
and verify bounded graph results, inventory-correlation hints, error states, and retry behavior according to
the backend contract.

**Acceptance Scenarios**:

1. **Given** no root identity is provided, **When** the user loads the graph view, **Then** the UI requests
   the full graph snapshot and renders returned nodes and edges.
2. **Given** caller-provided `depth` and `limit` values, **When** the user runs a query, **Then** the UI
   displays the applied request values and the `truncated` signal from the response.
3. **Given** the backend returns `400`, `401`, `403`, or `503`, **When** the UI handles the response,
   **Then** the page shows clear error messaging with actionable retry guidance and does not crash.
4. **Given** the backend is temporarily unavailable and later recovers, **When** the user retries from the
   UI, **Then** normal graph results render without requiring a full page restart.
5. **Given** graph node identities differ from inventory numeric IDs, **When** graph results are rendered,
   **Then** the UI shows inventory correlation details when node identities can be matched.

### Edge Cases

- Self-communication (same source and destination identity) is valid and represented as a self-link.
- Deep neighborhood requests with low size limits truncate by size before requested depth is exhausted.
- Events that cannot resolve a destination as internal identity are still represented via external host
  semantics when destination network evidence exists.
- Reordered or repeated deliveries must preserve structural uniqueness and monotonic relationship updates.
- When traversal depth and node limit are both exceeded, limit enforcement takes precedence for payload
  sizing and `truncated` MUST be `true`.

## Terminology

- **Identity**: Canonical identifier used to represent an endpoint in graph processing and querying.
- **Device**: Internal inventory-managed identity represented with `:Device`.
- **External Host**: Non-inventory destination represented as `:Device:ExternalHost`.
- **Node**: Graph representation of either a Device or External Host identity.
- **Relationship**: Directed `COMMUNICATED_WITH` edge keyed by `(sourceIdentity, destinationIdentity, protocol)`.
- **Valid Enriched Session Event**: Event containing, at minimum, `sourceIdentity`, `protocol`,
  `detectedAt`, and destination evidence (`destinationIdentity` or destination network evidence such as
  `destinationIp`). Events missing these minimum fields are dropped and counted in diagnostics.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST project each valid enriched session event into a communication graph as a
  relationship from source identity to destination identity.
- **FR-002**: Communication relationship identity MUST be unique by source identity, destination
  identity, and protocol.
- **FR-003**: Repeated observations for an existing relationship identity MUST increment communication
  weight, advance last-observed time to the maximum observed value, and preserve first-observed time.
- **FR-004**: Projection behavior MUST be idempotent under at-least-once event delivery so duplicate
  delivery does not duplicate graph structure.
- **FR-005**: The system MUST represent unresolved destinations as external-host identities while keeping
  internal device identities distinct for cleanup and traversal behavior.
- **FR-006**: The system MUST expose a graph retrieval capability where callers provide root identity and
  optional traversal controls, returning nodes, edges, and truncation status.
- **FR-007**: Graph retrieval depth MUST be capped by configuration even when callers request higher
  values.
- **FR-008**: Graph retrieval results MUST enforce a configurable size limit and signal truncation when
  the limit is reached.
- **FR-009**: A scheduled retention process MUST run every 24 hours and remove communication
  relationships older than 90 days by last-observed time.
- **FR-010**: After relationship pruning, external-host identities with no inbound communication links
  MUST be removed.
- **FR-011**: Internal device identities MUST NOT be removed by graph retention; their authority remains
  in device inventory.
- **FR-012**: Graph store outages MUST return a meaningful service-unavailable error for graph retrieval.
- **FR-013**: Graph store outages MUST NOT degrade unrelated device/session capabilities.
- **FR-014**: The graph retrieval endpoint MUST require authenticated access and MUST enforce role-based authorization. In this feature scope, the allowed read roles are `admin`, `analyst`, `auditor`, and `integration`.
- **FR-015**: The retention process MUST run only on its scheduled cadence in this feature; manual
  on-demand execution is out of scope.
- **FR-016**: Projection write failures MUST use bounded retries; after retry exhaustion, the system
  MUST emit operational diagnostics and continue processing subsequent events.
- **FR-017**: The feature MUST emit structured logs and metrics (counters and latency) for projection
  and retention execution paths to support operational monitoring and incident diagnosis.
- **FR-018**: The frontend MUST provide a graph exploration view that supports full snapshot retrieval
  without root identity and root-filtered retrieval with optional depth/limit inputs.
- **FR-019**: The graph UI MUST render returned nodes, edges, and truncation state in a way that allows
  operators to inspect communication neighborhoods without reading raw JSON.
- **FR-020**: The graph UI MUST provide explicit handling for `400`, `401`, `403`, and `503` responses
  with user-facing messages and a retry path.
- **FR-021**: The graph UI MUST preserve currently displayed results when a subsequent refresh fails and
  surface the refresh failure as non-destructive feedback.
- **FR-022**: The graph UI MUST remain isolated from device inventory management behavior so failures in
  graph retrieval do not degrade existing inventory workflows.
- **FR-023**: The backend MUST expose a bounded full-graph snapshot retrieval capability that does not
  require `rootDeviceId` and returns nodes, edges, and truncation status.
- **FR-024**: The graph UI MUST present inventory-correlation hints for graph nodes when identity matching
  is possible from available inventory data.

### Operational Parameters & Contracts

- **OP-001 (Query Defaults)**: If request `depth` is omitted, the service uses default depth `1`. If
  request `limit` is omitted, the service uses default limit `200`.
- **OP-002 (Query Bounds)**: `depth` maximum is `3`. `limit` maximum is `500`. Values above max are
  clamped; values below `1` are rejected as invalid request.
- **OP-003 (Retry Policy)**: Projection write retries are bounded to `maxRetries = 3`, exponential
  backoff base `200ms`, and per-attempt timeout `2s`.
- **OP-004 (Error Payload for FR-012)**: Service-unavailable responses for graph retrieval include
  `code`, `message`, `traceId`, and `timestampUtc`.
- **OP-005 (Auth Failure Semantics)**: Unauthenticated calls return `401`; authenticated calls with roles
  outside the allowed read-role set return `403`; authenticated calls outside valid request shape
  constraints return `400` with validation error details.
- **OP-006 (Retention Boundary Rule)**: Retention deletes edges where `lastSeen < cutoffUtc`
  (`cutoffUtc = nowUtc - 90 days`) and retains edges where `lastSeen == cutoffUtc`.
- **OP-007 (Retention Scheduling Tolerance)**: The 24-hour retention cadence allows scheduler drift up to
  +/- 10 minutes without violating FR-009/FR-015.
- **OP-008 (Observability Dimensions)**: Projection and retention metrics/logs include, at minimum:
  operation name, outcome (success/retry/failure), latencyMs, processedCount, and trace/correlation id.
- **OP-009 (Snapshot Bounds)**: Full-graph snapshot retrieval applies the same node-limit defaults/caps as
  neighborhood retrieval (`default=200`, `max=500`) and returns `truncated=true` when the cap is reached.

### Dependency Requirements

- **DR-001**: If upstream enriched-session events are unavailable, the system does not fabricate graph
  records and emits an operational signal indicating input starvation.
- **DR-002**: If incoming enriched events fail minimum validity checks, the system skips projection for
  those events, increments invalid-event counters, and continues with subsequent events.
- **DR-003**: Connector/graph-store dependency failures are isolated to graph projection/retrieval paths
  and MUST NOT alter non-graph endpoint behavior.

### Key Entities *(include if feature involves data)*

- **Enriched Session Event**: A validated traffic event carrying source/destination identity context and
  protocol needed for communication projection.
- **Communication Node**: A graph node representing either an internal device identity or an external
  host identity.
- **Communication Relationship**: A directed relationship between two communication nodes, keyed by
  source, destination, and protocol, with first/last seen and cumulative weight semantics.
- **Graph Query Result**: A bounded subgraph response containing nodes, edges, and a truncation flag.
- **Retention Sweep Outcome**: The scheduled cleanup result describing removed stale relationships and
  removed orphan external-host nodes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation runs, median projection delay from enriched event receipt to visible graph
  relationship is 2 seconds or less.
- **SC-002**: Replaying the same event payload twice results in no increase of node/relationship counts
  beyond expected identity-unique structure.
- **SC-003**: After retention execution, 100% of sampled relationships older than 90 days are absent.
- **SC-004**: After retention execution, 100% of sampled orphan external-host nodes are absent.
- **SC-005**: During graph-store outage simulation, graph retrieval returns service-unavailable responses
  while sampled non-graph device/session operations maintain successful responses.
- **SC-006**: In validation runs, projection and retention flows emit structured logs plus metrics for
  processed events, retries, failures, and execution latency in 100% of sampled scenarios.
- **SC-007**: In validation runs, operators can complete a graph lookup workflow (load full snapshot or
  enter root, run query, inspect nodes/edges, interpret truncation/error state) in under 60 seconds for
  95% of sampled attempts.

### Measurement Protocol

- **MP-001 (SC-001 timing points)**: Measurement starts at consumer receipt timestamp and ends at first
  successful graph query visibility of the projected relationship.
- **MP-002 (SC-001 sampling window)**: Median is computed over at least 500 consecutive valid events
  under nominal test load.
- **MP-003 (SC-003/SC-004 sampling)**: Validation samples at least 100 stale relationships and 100 orphan
  external-host candidates after a retention run.
- **MP-004 (SC-003/SC-004 pass threshold)**: Pass requires 100% compliance in sampled set, with any miss
  treated as failure.
- **MP-005 (SC-005 evidence)**: Validation evidence includes one failed graph request (`503`) and at
  least two successful non-graph requests in the same outage window.

### FR-to-SC Traceability

| Requirement | Primary Success Criterion |
|-------------|---------------------------|
| FR-001, FR-002, FR-003, FR-004, FR-005 | SC-001, SC-002 |
| FR-006, FR-007, FR-008, FR-014 | SC-005 |
| FR-009, FR-010, FR-011, FR-015 | SC-003, SC-004 |
| FR-012, FR-013 | SC-005 |
| FR-016, FR-017 | SC-006 |
| FR-018, FR-019, FR-020, FR-021, FR-022, FR-023, FR-024 | SC-007, SC-005 |

## Assumptions

- Historical design decisions imported from prior documentation are treated as baseline constraints for
  this feature unless explicitly superseded in this spec.
- Graph projection consumes enriched session events already produced by prior session-processing
  capabilities.
- Device inventory remains the authoritative source for internal device identity existence and lifecycle.
- The communication graph UI consumes the backend retrieval contract and remains additive to existing
  inventory management capabilities.
- Backfilling communication links older than event-retention windows is outside current scope and would
  require a separate replay/backfill capability.

## Non-Functional Scope Boundaries

- This feature does not introduce SLO enforcement automation; it defines measurable criteria and required
  signals for operational validation.
- This feature does not include per-role data filtering beyond the allowed read-role set; fine-grained
  role scoping is deferred to a future authorization hardening feature.
- This feature does not include connector self-healing orchestration beyond bounded retries and failure
  diagnostics in projection paths.
