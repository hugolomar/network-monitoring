# Scenario View

**Purpose**: Produce the UC semantics for the architecture workflow. This view is the source for the logical, process, development, and physical views.

## Architecture Intent

Stabilize end-to-end platform meaning for network observation value delivery: detect network activity,
publish durable platform facts, transform those facts into operationally useful inventories, and let
operators validate and manage outcomes without coupling user workflows to capture internals.

## Core Tensions

| Tension | Current Tradeoff Direction | Scenario Consequence |
|---------|----------------------------|----------------------|
| Fast operator feedback vs durable platform integration | Keep immediate operator-visible validation while preserving asynchronous publication as the integration backbone | A scenario can succeed locally first, then expand to platform consumers without redefining actor goals |
| Shared semantic consistency vs independently delivered increments | Use stable domain meaning and contract-first boundaries while delivering feature slices independently | New scenarios must reuse existing fact meaning rather than introduce alternate interpretations |
| Manual operator control vs automated ingestion path | Keep manual management and automated discovery as separate scenario entry points that converge at one authoritative inventory boundary | Both paths can coexist without competing ownership of device identity and lifecycle |

## Stable Boundaries

| Boundary | Must Remain Stable Because | Explicitly Does Not Cover |
|----------|----------------------------|---------------------------|
| Observation-to-fact boundary | Detection scenarios rely on stable event meaning for downstream reuse and auditability | Long-term forensic analytics beyond defined query workflows |
| Authoritative device inventory boundary | Manual and automated device scenarios must converge on one source of truth | Independent persistence owned by UI or ingestion intermediaries |
| User interaction boundary | Operator workflows need inventory visibility and controlled creation without operational internals | Direct control of capture processes or stream internals from UI interactions |

## Change Axes

| Expected Change | Isolated By | Scenario Impact |
|-----------------|-------------|-----------------|
| New consumers of emitted facts | Contract-first event and query boundaries | Existing operator and maintainer scenarios remain stable while consumers evolve |
| Device validation and consolidation policy evolution | Authoritative inventory boundary with shared domain semantics | Scenario outcomes stay coherent even when policy detail changes |
| Operational topology and deployment form | Separation between participant goals and runtime placement | Scenario semantics remain valid across local, staged, and production-class environments |

## Invariants

| Invariant | Scenario Evidence | Risk If Violated |
|-----------|-------------------|------------------|
| Session and device facts retain shared meaning across all scenarios | Detection, ingestion, inventory, and management scenarios all depend on the same identity and lifecycle interpretation | Cross-scenario contradictions produce invalid investigations and unreliable automation |
| Invalid inputs do not terminate the core flow for later valid inputs | Detection and ingestion scenarios explicitly require continued processing after invalid evidence | One bad input can block operational visibility and downstream delivery |
| Repeated evidence for the same identity consolidates instead of fragmenting outcomes | Device discovery, ingestion idempotency, and inventory visibility scenarios expect one logical identity trajectory | Duplicate logical entities undermine trust and operational correctness |

## Non-goals / Anti-patterns

| Non-goal / Anti-pattern | Why It Is Out of Scope or Harmful |
|-------------------------|-----------------------------------|
| Collapsing all participant concerns into one monolithic scenario | Blurs authority boundaries and removes the ability to evolve slices independently |
| Letting presentation workflows own persistence or stream semantics | Violates authoritative boundary and creates conflicting fact ownership |
| Treating local validation output as a substitute for durable platform facts | Prevents reliable downstream use and weakens auditability requirements |

## Actors and Participants

| Actor / Participant | Goal | Responsibility | Boundary |
|---------------------|------|----------------|----------|
| Network Operator | Confirm capture and discovery behavior in running environments | Observe emitted outcomes, validate progress, act on diagnostics | Human operational interaction |
| Platform Operator | Ensure facts are available for downstream systems and governance | Validate publication and ingestion continuity across runtime participants | Platform operations boundary |
| Device Manager | Maintain useful inventory state including manual additions | Create and review device state through controlled management workflows | User-facing management boundary |
| Probe Runtime Participant | Transform observations into validated platform facts | Detect sessions/devices, reject invalid evidence, emit stable facts | Observation and detection boundary |
| Integration Runtime Participant | Bridge emitted device facts into authoritative inventory | Validate intake identity consistency, handle delivery outcomes, report processing state | Ingestion handoff boundary |
| Inventory Runtime Participant | Preserve authoritative device lifecycle state | Accept valid intake, consolidate repeated evidence, expose inventory facts | Authoritative inventory boundary |

## Use Cases

| Use Case | Actor | Goal | Preconditions | Scope Boundary |
|----------|-------|------|---------------|----------------|
| UC-01 Validate Session Detection | Network Operator | Confirm session observations become operator-visible facts | Observation source is active and capture runtime is operating | Detection visibility only |
| UC-02 Access Session History | Platform Operator | Query previously emitted session facts for investigation | Session facts have been published and projected for retrieval | Historical query behavior, not capture internals |
| UC-03 Validate Device Discovery | Network Operator | Confirm device detection and consolidation behavior | Discovery runtime receives valid and invalid observation evidence | Discovery semantics only |
| UC-04 Ingest Device Facts | Platform Operator | Move emitted device facts into authoritative inventory | Device facts are available on the asynchronous backbone | Ingestion and intake continuity |
| UC-05 Manage Device Inventory | Device Manager | Review and create device records through managed workflows | Authoritative inventory participant is available | User-facing inventory lifecycle actions |

## Scenario Paths

| Scenario | Main Path | Successful Outcome | Alternative / Failure Branches |
|----------|-----------|--------------------|--------------------------------|
| Session detection visibility | Observation evidence enters probe runtime, valid evidence is transformed into session facts, operator-visible flow reports results | Operator confirms session fact emission without manual reconstruction | No traffic yields empty-but-valid outcome; invalid evidence is rejected while processing continues |
| Session history access | Published session facts are projected to queryable history, operator applies filtering criteria, matching facts are returned | Historical session facts are retrievable with stable semantic meaning | Projection delay requires expectation management; no matches returns empty-but-valid outcome |
| Device discovery and consolidation | Discovery evidence is validated, repeated identity evidence is consolidated, device facts are emitted | Stable device identity timeline is observable and reusable | Invalid identity evidence is rejected with diagnostics; temporary publication disruption is surfaced without semantic drift |
| Device ingestion to inventory | Device facts are consumed, identity consistency is validated, authoritative inventory accepts valid intake | Device fact becomes authoritative inventory state without duplicate identity creation | Delivery failures trigger bounded retry/degradation; irrecoverable invalid intake is rejected and visible |
| Device management workflow | Inventory participant provides current state, user submits creation intent, authoritative boundary applies validation and consolidation | User sees consistent inventory state and successful or idempotent outcomes | Validation errors and temporary backend unavailability are recoverable and do not corrupt existing state |

## Acceptance Semantics

| Acceptance Scenario | Observable Result | Must Hold | Not Covered |
|---------------------|-------------------|-----------|-------------|
| Session facts remain consistent across live and historical paths | Same logical session meaning appears in operator-visible and historical retrieval outcomes | Contract-stable semantics across participants | Low-level storage or indexing implementation |
| Device identity remains singular across discovery, ingestion, and management | One logical device identity trajectory is visible despite repeated evidence | Consolidation and idempotent handling preserve authoritative state | Future identity federation across external domains |
| Invalid evidence is handled without stopping useful work | Diagnostics show rejection while later valid evidence still produces outcomes | Failure isolation and continuation guarantees | Detailed fault taxonomy for every infrastructure vendor |
| Manual and automated creation converge to one authoritative result | User-visible inventory reflects authoritative decisions from both paths | Unified validation authority for device state | Role/permission policy details beyond current scenario scope |

## Scenario Gaps

| Gap | Affected Scenario | Why It Matters |
|-----|-------------------|----------------|
| Cross-actor authorization semantics are deferred | UC-05 Manage Device Inventory, UC-02 Access Session History | Without explicit actor authorization policy, governance obligations cannot be fully validated at scenario level |
| Explicit freshness expectation for publication-to-history availability remains qualitative | UC-02 Access Session History | Lacking bounded freshness semantics weakens operator expectation alignment for investigations |
| Cross-environment operational fallback for prolonged asynchronous backbone outage is unspecified | UC-04 Ingest Device Facts | Extended outage behavior impacts continuity expectations for authoritative inventory updates |

## Prohibited Content

Do not write architecture components, class designs, APIs, database tables, implementation tasks, test strategy, deployment scripts, or framework choices here.
