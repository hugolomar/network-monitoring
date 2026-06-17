# Logical View

**Input**: `.specify/memory/architecture-scenario-view.md`

**Purpose**: Derive capability boundaries, domain objects, states, relationships, and invariants from the scenario view.

## Architecture Intent

Preserve a single authoritative domain meaning across detection, ingestion, inventory, and management
capabilities while keeping responsibility ownership explicit enough to prevent semantic drift between
runtime participants and user-facing workflows.

## Core Tensions

| Tension | Current Tradeoff Direction | Logical Consequence |
|---------|----------------------------|---------------------|
| Shared domain authority vs projection-specific representations | Keep authoritative meaning in shared domain while allowing bounded projections for retrieval and interaction | Projection objects may optimize usage but cannot redefine core fact meaning |
| Unified inventory authority vs distributed fact producers | Treat producers as contributors, not owners, of authoritative device state | Ingestion and management depend on one ownership boundary for acceptance decisions |
| Incremental slice delivery vs long-lived model coherence | Permit phased capabilities while binding all slices to explicit invariants | New capabilities must reference existing object semantics before introducing extensions |

## Stable Boundaries

| Boundary | Must Remain Stable Because | Explicitly Does Not Own |
|----------|----------------------------|-------------------------|
| Shared domain authority | Session and device meaning must remain consistent across all slices | Runtime transport, deployment topology, and user interaction details |
| Fact publication boundary | Producers must emit reusable facts without owning downstream lifecycle | Authoritative persistence and user-facing inventory composition |
| Authoritative inventory boundary | Device lifecycle consolidation and idempotent acceptance require one decision point | Capture policy and asynchronous transport semantics |
| Interaction boundary | User workflows need stable consumption and creation semantics | Internal reconciliation logic of non-user-facing participants |

## Change Axes

| Expected Change | Isolated By | Logical Impact |
|-----------------|-------------|----------------|
| New fact consumers and query paths | Contract-first publication and retrieval boundaries | Additional consumers can be introduced without changing object meaning |
| Device policy refinements for consolidation and validation | Authoritative inventory ownership and invariant set | Lifecycle rules evolve while preserving identity singularity |
| Additional management capabilities | Interaction boundary over authoritative state | User experience can expand without fragmenting domain authority |

## Invariants

| Invariant | Source Scenario / Object / State | Risk If Violated |
|-----------|----------------------------------|------------------|
| Session fact meaning remains stable between live emission and historical retrieval | UC-01 Validate Session Detection, UC-02 Access Session History, Session Fact object | Investigations rely on inconsistent semantics and cannot be trusted |
| Device identity is singular by normalized identity semantics across all boundaries | UC-03 Validate Device Discovery, UC-04 Ingest Device Facts, UC-05 Manage Device Inventory, Device Identity object | Duplicate logical devices or conflicting merges degrade inventory trust |
| Invalid evidence is explicitly rejected before authoritative state mutation | UC-03 and UC-04 rejection paths, Intake Candidate state | Corrupt or ambiguous data enters authoritative flows and propagates downstream |
| Authoritative inventory state is the only source for user-facing device management outcomes | UC-05 Manage Device Inventory, Device Inventory State object | UI or ingestion intermediaries can diverge from true platform state |

## Non-goals / Anti-patterns

| Non-goal / Anti-pattern | Why It Is Out of Scope or Harmful |
|-------------------------|-----------------------------------|
| Defining independent device identity semantics per capability | Creates cross-boundary contradictions and breaks consolidation guarantees |
| Treating projections as authoritative sources | Reverses ownership and allows stale or partial facts to drive decisions |
| Embedding user workflow ownership in transport-facing capabilities | Couples interaction concerns to infrastructure concerns and reduces evolvability |

## Capability Boundaries

| Capability / Boundary | Responsibility | Input | Output | Explicitly Does Not Own | Scenario Source |
|-----------------------|----------------|-------|--------|--------------------------|-----------------|
| Observation Interpretation | Convert observation evidence into candidate session/device facts | Observation evidence | Validated detection candidates and rejection diagnostics | Authoritative inventory lifecycle decisions | UC-01, UC-03 |
| Fact Publication | Expose validated facts as reusable asynchronous facts and immediate local observability | Validated detection candidates | Published facts and operator-visible confirmation | Historical query projection ownership and authoritative inventory mutation | UC-01, UC-02, UC-03, UC-04 |
| Historical Query Projection | Provide retrieval-oriented session history representation from published facts | Published session facts | Queryable historical session facts | Redefinition of session semantics or capture policy | UC-02 |
| Intake Validation and Bridging | Validate consumed device facts for authoritative acceptance | Published device facts | Authoritative intake submissions and rejection outcomes | Device persistence authority and user interaction ownership | UC-04 |
| Authoritative Device Inventory | Accept, consolidate, and preserve device lifecycle state | Intake submissions and management intents | Canonical inventory state and management-visible outcomes | Observation capture and asynchronous publication ownership | UC-04, UC-05 |
| User Device Management Interaction | Present inventory and submit manual creation intents | Canonical inventory state and user intents | User-visible inventory outcomes and creation requests | Persistence authority and transport semantics ownership | UC-05 |

## Domain Objects and Relationships

| Object | Meaning | Owning Capability | Key Relationships | Fact Source | Invariants |
|--------|---------|-------------------|-------------------|-------------|------------|
| Session Fact | Stable representation of network communication evidence for operations and investigation | Observation Interpretation (meaning), Fact Publication (distribution) | Derived from observation evidence, projected into historical query representation | Observation evidence validated in detection flow | Semantic equivalence across live and historical paths |
| Device Identity | Stable logical identifier for a discovered or managed device | Shared domain authority consumed by discovery, intake, and inventory boundaries | Anchors Device Fact, Intake Candidate, and Inventory Device State | Detection evidence and management intent interpreted under shared rules | Identity singularity across all capabilities |
| Device Fact | Published representation of validated device evidence for downstream processing | Fact Publication | Consumed by intake validation and correlated to inventory state | Validated detection candidates | Cannot conflict with shared domain identity semantics |
| Intake Candidate | Device fact prepared for authoritative acceptance decision | Intake Validation and Bridging | Derived from Device Fact, accepted into Inventory Device State or rejected | Consumed published device facts | Ambiguous or invalid candidates cannot advance to authoritative state |
| Inventory Device State | Authoritative lifecycle view of a device | Authoritative Device Inventory | Updated by accepted Intake Candidate and management intent; exposed to interaction boundary | Accepted authoritative decisions | One logical state per device identity with deterministic consolidation |
| Management Intent | User-submitted device creation/update request semantics | User Device Management Interaction | Evaluated by authoritative inventory alongside ingestion-based intake | User operator participation | Must pass same authoritative validation path as automated intake |

## State and Lifecycle

| Object / Flow | State | Entered When | Exited When | Forbidden Transition | Responsible Boundary |
|---------------|-------|--------------|-------------|----------------------|----------------------|
| Session Fact Flow | Candidate Observation | Observation evidence is interpreted | Evidence is validated or rejected | Candidate Observation directly to Historical Query without validated fact publication | Observation Interpretation |
| Session Fact Flow | Validated Session Fact | Candidate satisfies shared semantics | Fact is published and projected or archived by downstream consumers | Validated Session Fact to contradictory projection semantics | Fact Publication |
| Device Fact Flow | Discovery Candidate | Device evidence is interpreted | Candidate is validated or rejected | Discovery Candidate directly to Inventory Device State | Observation Interpretation |
| Device Fact Flow | Published Device Fact | Valid candidate is emitted as reusable fact | Consumed for intake validation | Published Device Fact to authoritative acceptance without intake validation | Fact Publication and Intake Validation and Bridging |
| Inventory Device State | Accepted Canonical State | Intake candidate or management intent is accepted | Consolidated by later valid evidence or exposed to users | Accepted Canonical State split into parallel identities for same device | Authoritative Device Inventory |
| Management Intent | Submitted Intent | User requests manual creation/update | Accepted into canonical state or rejected with reason | Submitted Intent bypassing authoritative validation | User Device Management Interaction and Authoritative Device Inventory |

## Logical Decisions

| Decision | Scope | Owner / Boundary | Affected Objects or Flows | Consequence |
|----------|-------|------------------|---------------------------|-------------|
| Retain shared domain authority as semantic source for session and device meaning | Project-level | Shared domain authority boundary | Session Fact, Device Identity, Device Fact, Inventory Device State | All capabilities must map to one semantic baseline before extending behavior |
| Converge automated ingestion and manual management into one authoritative acceptance boundary | Device lifecycle and operations | Authoritative Device Inventory | Intake Candidate, Management Intent, Inventory Device State | Validation and consolidation remain consistent regardless of source path |
| Treat published facts as reusable but non-authoritative for persistence decisions | Cross-participant collaboration | Fact Publication and Intake Validation and Bridging | Device Fact, Session Fact, Intake Candidate | Downstream adaptation is allowed without transferring ownership of truth |

## Logical Gaps

| Gap | Affected Capability / Object | Why It Matters |
|-----|------------------------------|----------------|
| Authorization policy semantics for query and management participants are not fully modeled | User Device Management Interaction, Historical Query Projection | Boundary responsibilities for who can access or mutate specific facts remain under-specified |
| Canonical conflict-resolution semantics for concurrent management and ingestion updates are qualitative only | Inventory Device State | Without explicit precedence rules, deterministic consolidation could diverge across deployments |
| Project-level retention and lifecycle horizon for historical session facts is not explicitly bounded | Historical Query Projection, Session Fact | Long-term lifecycle assumptions may vary and impact investigative guarantees |

## Prohibited Content

Do not write classes, DTOs, database tables, fields, method names, endpoints, schemas, or implementation data structures here.
