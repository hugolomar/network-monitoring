# Development View

**Input**: `.specify/memory/architecture-logical-view.md`, `.specify/memory/architecture-process-view.md`

**Purpose**: Derive architecture-level components, package boundary intent, contract/artifact semantics, and dependency rules from logical and process views.

## Architecture Intent

Preserve component and package-level authority boundaries that encode shared domain semantics, runtime
handoff contracts, and canonical inventory ownership while allowing incremental expansion through
well-defined dependencies rather than cross-boundary shortcuts.

## Core Tensions

| Tension | Current Tradeoff Direction | Development Consequence |
|---------|----------------------------|-------------------------|
| Fast feature delivery vs boundary purity | Prefer incremental modules with explicit inward dependency discipline | New capability packages must align to stable abstraction boundaries before extension |
| Shared domain reuse vs participant-specific adaptation | Keep core semantic abstractions centralized while allowing adapter-layer translations | Integration and interaction packages consume shared semantics without redefining them |
| Cross-cutting observability/documentation needs vs component cohesion | Keep quality obligations within each boundary rather than centralizing bypass layers | Every component boundary carries its own contract clarity and review obligations |

## Stable Boundaries

| Boundary | Must Remain Stable Because | Explicitly Must Not Own |
|----------|----------------------------|-------------------------|
| Shared Domain Package Boundary | All runtime and user-facing packages require one semantic authority | Transport protocols, deployment topology, and participant orchestration policy |
| Detection Capability Package Boundary | Observation interpretation and local publication semantics must remain coherent | Authoritative inventory persistence decisions |
| Integration Capability Package Boundary | Consumption/bridging behavior isolates asynchronous handoff concerns | Device lifecycle authority and user interaction ownership |
| Inventory Capability Package Boundary | Canonical acceptance and consolidation policy must remain centralized | Capture internals and asynchronous transport ownership |
| Management Capability Package Boundary | User-oriented interaction workflows must remain decoupled from internals | Direct persistence internals and stream infrastructure ownership |

## Change Axes

| Expected Change | Isolated By | Development Impact |
|-----------------|-------------|--------------------|
| Additional feature slices for analytics, security, or governance | Layered capability package boundaries with explicit contracts | New packages can be introduced without violating existing ownership lines |
| Evolution of contract semantics between participants | Contract/artifact registry discipline and compatibility expectations | Producers and consumers may evolve independently with explicit compatibility review |
| Future operational hardening | Dependency rules keeping infrastructure adaptation outside core semantic boundaries | Deployment or security adapters can change without rewriting domain meaning |

## Invariants

| Invariant | Source Boundary / Contract / Dependency Rule | Risk If Violated |
|-----------|----------------------------------------------|------------------|
| Domain abstractions are authoritative for session/device meaning | Shared Domain Package Boundary + dependency inward rule | Participant packages diverge in semantic interpretation |
| Canonical device state authority is centralized | Inventory Capability Package Boundary + canonical acceptance process links | Duplicate or contradictory authoritative states emerge |
| User and integration packages consume contracts, not persistence internals | Interaction and integration boundaries + forbidden dependency directions | External-facing packages become tightly coupled and brittle |
| Contract evolution is explicit and compatibility-aware | Contract/artifact semantics across detection, integration, inventory, and management | Runtime participants drift and break end-to-end collaboration |

## Non-goals / Anti-patterns

| Non-goal / Anti-pattern | Why It Is Out of Scope or Harmful |
|-------------------------|-----------------------------------|
| Building a single package that combines capture, ingestion, persistence, and UI concerns | Destroys boundary ownership and prevents independent evolution |
| Allowing adapter/infrastructure packages to become semantic authorities | Inverts clean architecture direction and weakens domain consistency |
| Treating documentation/quality obligations as optional per package | Violates constitutional quality gates and creates uneven maintainability |

## Architecture-Level Components

| Component / Capability Package | Responsibility | Input / Output Boundary | Collaborators | Explicitly Must Not Own | Source View Evidence |
|--------------------------------|----------------|-------------------------|---------------|--------------------------|----------------------|
| Shared Domain Core | Define canonical meaning and invariants for core facts and identities | Inward semantic abstractions consumed by all capability packages | Detection, Integration, Inventory, Management packages | Transport adaptation, deployment concerns, user interaction flows | Logical shared authority boundary, invariants on semantic consistency |
| Detection Capability Package | Interpret observations and publish validated facts with local operator feedback | Observation evidence in, session/device facts and diagnostics out | Shared Domain Core, Contract Registry, Asynchronous Handoff Boundary | Canonical inventory ownership | Scenario UC-01/UC-03, Process RL-01/RL-02 |
| Historical Query Capability Package | Provide bounded retrieval view over session fact history | Query intent in, historical session facts out | Shared Domain Core, Contract Registry, Publication Projection Boundary | Redefining detection semantics | Scenario UC-02, Logical projection boundary |
| Integration Capability Package | Consume published device facts and bridge them into authoritative acceptance candidates | Published device facts in, intake submissions and rejection outcomes out | Shared Domain Core, Contract Registry, Inventory Capability Package | Canonical persistence authority, UI ownership | Scenario UC-04, Process RL-04 |
| Inventory Capability Package | Accept and consolidate canonical device state and expose inventory outcomes | Intake submissions/management intents in, canonical inventory outcomes out | Shared Domain Core, Integration Capability Package, Management Capability Package | Observation capture and transport publication ownership | Scenario UC-04/UC-05, Process RL-05 |
| Management Capability Package | Present inventory and submit manual management intents | Canonical inventory state in, management intents out | Inventory Capability Package, Shared Domain Core | Persistence internals and asynchronous backbone ownership | Scenario UC-05, Process RL-06 |

## Package Boundary Intent

| Package / Boundary | Abstraction Level | Owned Concepts | May Depend On | Must Not Depend On | Evolution Rule |
|--------------------|-------------------|----------------|---------------|--------------------|----------------|
| Shared Domain Core | Domain semantics and invariants | Session/device identity and lifecycle semantics | None within project semantic hierarchy | Capability-specific adapters and external runtimes | Changes require cross-capability compatibility review |
| Capability Application Boundaries | Use-case and collaboration semantics | Participant responsibilities and flow-level decisions | Shared Domain Core abstractions | Infrastructure-bound implementation detail as semantic source | Evolve by adding use cases, not by crossing ownership lines |
| Infrastructure Adaptation Boundaries | Technical realization of capability contracts | Transport/storage/hosting adaptation for each capability | Corresponding capability application boundary and shared abstractions | Other capability internals across authority boundaries | Adaptation can change while preserving contract semantics |
| Interaction Boundaries | Operator-facing consumption and intent submission semantics | View state and management intent semantics | Inventory capability contracts and shared semantics | Direct infrastructure ownership beyond interaction concerns | UX changes must preserve authoritative acceptance semantics |

## Contracts and Artifacts

| Contract / Artifact | Semantics | Producer | Consumer | Lifecycle | Architecture Consequence |
|---------------------|-----------|----------|----------|-----------|--------------------------|
| Session Fact Contract | Canonical session fact meaning for live and historical use | Detection Capability Package | Historical Query Capability Package and other future consumers | Versioned, compatibility-aware evolution | Enables decoupled consumers while preserving semantic continuity |
| Device Fact Contract | Canonical detected device meaning for asynchronous collaboration | Detection Capability Package | Integration Capability Package and future consumers | Versioned, compatibility-aware evolution | Establishes one handoff language for downstream authoritative intake |
| Intake Submission Contract | Canonical request semantics for authoritative device acceptance | Integration Capability Package and Management Capability Package | Inventory Capability Package | Stable with explicit evolution decisions | Converges automated and manual origins at one acceptance boundary |
| Inventory Exposure Contract | Canonical inventory retrieval semantics for interaction workflows | Inventory Capability Package | Management Capability Package and future clients | Stable read semantics with additive evolution direction | User-facing workflows remain independent of persistence internals |

## Dependency Rules

| Rule | Allowed Direction | Forbidden Direction | Reason | Risk If Violated |
|------|-------------------|---------------------|--------|------------------|
| DR-01 Inward Domain Dependency | Capability packages depend inward on shared domain abstractions | Shared domain depends on capability or infrastructure adaptation boundaries | Preserves semantic authority and clean boundary direction | Domain meaning becomes coupled to runtime specifics |
| DR-02 Authority Respect | Integration and management packages depend on inventory contracts for canonical outcomes | Inventory package depends on interaction-specific or integration-specific internals for authority decisions | Maintains one canonical acceptance authority | Competing sources of truth emerge |
| DR-03 Contract Mediation | Cross-capability collaboration occurs through explicit contracts/artifacts | Direct internal package coupling across capability boundaries | Supports independent evolution and compatibility governance | Tight coupling causes cascading change impact |
| DR-04 Interaction Isolation | Interaction package depends on inventory exposure semantics only | Interaction package depending directly on transport/persistence infrastructure internals | Keeps UI concerns decoupled from operations internals | User workflows break under infrastructure changes |

## Development View Gaps

| Gap | Affected Component / Boundary | Why It Matters |
|-----|-------------------------------|----------------|
| Canonical governance location for architecture decision records across capability boundaries is implicit | All capability packages and contract evolution discipline | Without explicit ownership cadence, major tradeoff decisions may be inconsistently documented |
| Shared policy for backward compatibility horizon across all contract artifacts is not explicitly quantified | Session/Device Fact Contracts and Intake/Inventory contracts | Inconsistent compatibility expectations can produce uneven change safety |
| Boundary-level quality gate automation mapping remains high-level | All capability packages | Constitutional obligations are clear, but package-specific enforcement linkage is under-specified |

## Prohibited Content

Do not write source file paths, concrete package trees, classes, functions, implementation tasks, framework-specific wiring, or code generation notes here.
