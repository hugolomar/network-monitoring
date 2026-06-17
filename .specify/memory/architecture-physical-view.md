# Physical View

**Input**: `.specify/memory/architecture-process-view.md`, `.specify/memory/architecture-development-view.md`

**Purpose**: Derive deployment, hosting, external system, fact-source, observability, and operational boundaries from process and development views.

## Architecture Intent

Preserve deployable-unit independence and external collaboration boundaries so each runtime participant
can be operated, migrated, and observed independently while still maintaining end-to-end fact continuity
across asynchronous and authoritative boundaries.

## Core Tensions

| Tension | Current Tradeoff Direction | Physical Consequence |
|---------|----------------------------|----------------------|
| Local reproducibility vs production-grade hardening | Keep containerized deployables and local reference operation while deferring some hardening details | Runtime units remain portable across environments without redefining scenario semantics |
| Independent participant deployment vs coupled operational outcomes | Deploy participants separately but bind them through stable fact/contract boundaries | Failures can be isolated to runtime units while preserving traceable end-to-end flow |
| External dependency usage vs ownership of truth | Use external systems for transport/query/persistence roles while retaining explicit authoritative boundaries | Collaboration with external systems cannot redefine canonical domain facts |

## Stable Boundaries

| Boundary | Must Remain Stable Because | Explicitly Does Not Carry |
|----------|----------------------------|---------------------------|
| Probe runtime unit boundary | Detection and publication responsibilities must remain deployable independently | Authoritative inventory persistence decisions |
| Integration runtime unit boundary | Intake bridging must operate independently from both producer and canonical inventory units | Source-of-truth ownership for device lifecycle |
| Inventory backend runtime boundary | Canonical acceptance and retrieval semantics require one operational authority | Direct capture responsibilities and stream transport ownership |
| Management UI runtime boundary | User interaction must remain separable from backend and stream internals | Persistence and asynchronous backbone responsibilities |
| Asynchronous event backbone boundary | Decouples producer and consumer runtime lifecycles | Canonical persistence authority and user interaction policy |

## Change Axes

| Expected Change | Isolated By | Physical Impact |
|-----------------|-------------|-----------------|
| Runtime scale/profile differences across environments | Containerized deployable boundaries and configuration-driven collaboration | Units can be tuned or moved independently without semantic redesign |
| External system replacements for transport or query projection | Contract-stable collaboration boundaries | External substitution remains possible if exchanged fact semantics are preserved |
| Expanded observability and governance requirements | Fact-source traceability boundaries with participant-local diagnostics | Additional monitoring can be layered without cross-unit ownership drift |

## Invariants

| Invariant | Source Deployment / External / Fact Boundary | Risk If Violated |
|-----------|----------------------------------------------|------------------|
| Each deployable unit preserves its declared ownership boundary | Probe, integration, inventory, and management runtime boundaries | Operational coupling increases and architecture responsibilities blur |
| Asynchronous backbone remains a collaboration boundary, not a canonical authority boundary | External event backbone collaboration semantics | Consumers may treat transport state as source of truth and diverge from authoritative state |
| Authoritative inventory remains the canonical fact source for device management outcomes | Inventory backend runtime boundary and interaction collaboration | User-visible state diverges from canonical lifecycle decisions |
| Fact traceability spans producer, bridge, and authoritative acceptance outcomes | Fact/event observability requirements across runtime links | Failure diagnosis and compliance review lose end-to-end evidence |

## Non-goals / Anti-patterns

| Non-goal / Anti-pattern | Why It Is Out of Scope or Harmful |
|-------------------------|-----------------------------------|
| Binding all participants into one deployment unit for convenience | Removes independent operability and increases blast radius |
| Letting UI or integration units query external persistence internals directly | Breaks authoritative boundary and creates hidden couplings |
| Assuming external system availability as a permanent invariant | Prevents explicit degradation/closure design for collaboration failures |

## Deployment and Hosting Boundaries

| Runtime / Hosting Unit | Carries | Boundary | Depends On | Release / Migration Impact |
|------------------------|---------|----------|------------|----------------------------|
| Probe Runtime Unit | Observation interpretation and fact publication responsibilities | Detection and publication ownership boundary | Observation environment and asynchronous backbone availability | Can be released independently; downstream consumers rely on stable fact semantics |
| Integration Runtime Unit | Device fact consumption and authoritative intake bridging responsibilities | Intake validation and handoff boundary | Asynchronous backbone and authoritative inventory backend availability | Migration affects ingestion continuity but not canonical inventory authority |
| Inventory Backend Runtime Unit | Canonical device acceptance, consolidation, and retrieval responsibilities | Authoritative inventory boundary | Persistence substrate and collaboration with integration/management units | Release changes must preserve intake and retrieval contract continuity |
| Management UI Runtime Unit | Operator inventory interaction and manual intent submission responsibilities | Interaction boundary | Inventory backend availability | UI release can evolve user experience while preserving authoritative interaction semantics |
| Historical Query Runtime Unit | Session history retrieval responsibilities | Query projection boundary | Published session fact availability and projection substrate | Query evolution must preserve session semantic equivalence |

## External System Collaboration

| External System | Purpose | Exchanged Content | Authoritative Fact | Failure Impact | Isolation / Substitute Boundary |
|-----------------|---------|-------------------|--------------------|----------------|---------------------------------|
| Network Observation Environment | Provides raw evidence for detection scenarios | Observation evidence | Not authoritative for platform facts until validated | Reduced/no evidence causes empty detection outcomes | Detection boundary isolates interpretation from environment specifics |
| Asynchronous Event Backbone | Decoupled transport for published facts | Session/device facts and correlation semantics | Not canonical source of device lifecycle truth | Publication/consumption delays affect downstream freshness | Contract-stable handoff allows alternate transport implementations |
| Session History Projection Substrate | Supports retrieval-oriented session query availability | Projected session facts | Retrieval representation only; canonical meaning remains shared domain fact | Delays or unavailability impact investigative workflows | Query boundary isolates projection concerns from detection semantics |
| Device Inventory Persistence Substrate | Preserves canonical device lifecycle state | Canonical device state | Canonical source for device inventory outcomes (via inventory boundary) | Persistence unavailability blocks authoritative advancement | Inventory boundary can expose recoverable unavailability without semantic drift |

## Fact Sources and Observability

| Fact / Event | Authoritative Source | Observable Location | Consumers | Traceability Requirement |
|--------------|----------------------|---------------------|-----------|--------------------------|
| Session Fact Outcome | Detection/publication boundary under shared domain semantics | Detection runtime receipts and history retrieval outcomes | Operators and historical query participants | Must correlate live and historical semantics for the same logical session |
| Device Fact Publication Outcome | Detection/publication boundary | Detection and integration runtime receipts | Integration participant and platform operators | Must preserve identity semantics across publication and intake links |
| Authoritative Device State Outcome | Inventory backend boundary | Inventory backend and management interaction receipts | Device managers and downstream clients | Must distinguish accepted, consolidated, idempotent, and rejected outcomes |
| Cross-boundary Failure Closure Outcome | Each responsible runtime boundary on failure branch | Participant-local diagnostics and operator-visible status | Operators and maintainers | Must show detection boundary, responsible boundary, and closure status |

## Operations and Release Boundaries

| Operational Concern | Responsible Boundary | Trigger | Affected Views | Architecture Consequence |
|---------------------|----------------------|---------|----------------|--------------------------|
| Deployable independence maintenance | All runtime unit boundaries | New feature slice or deployment topology change | Development, Physical, Process | Changes must retain explicit runtime ownership and forbidden crossings |
| Contract compatibility governance | Producers and consumers of each cross-boundary contract | Contract evolution proposal | Development, Logical, Scenario, Physical | Compatibility decisions require explicit tradeoff documentation and review |
| Outage and degradation handling | Runtime boundary where failure is detected | External dependency unavailability | Process, Physical, Scenario | Failure must produce explicit closure without silently reassigning ownership |
| Environment portability validation | Probe, integration, inventory, and management runtime units | Environment migration or setup changes | Physical, Process | Local and production-class operation remain aligned at boundary semantics level |

## Physical View Gaps

| Gap | Affected Deployment / External Boundary | Why It Matters |
|-----|-----------------------------------------|----------------|
| Authoritative policy for prolonged asynchronous backbone outage escalation is not explicit | Asynchronous event backbone, integration runtime unit | Extended disruption handling may diverge operationally without explicit boundary-level policy |
| Cross-environment observability normalization expectations are qualitative | All runtime units and fact-source observability rows | Inconsistent diagnostics shape can weaken end-to-end traceability across environments |
| Security hardening responsibilities beyond baseline transport protection are deferred | Runtime collaboration with external systems | Deferred ownership details affect architecture readiness for stricter environments |

## Prohibited Content

Do not write Kubernetes YAML, cloud resource manifests, machine sizes, service SKUs, deployment scripts, runbooks, or concrete infrastructure configuration here.
