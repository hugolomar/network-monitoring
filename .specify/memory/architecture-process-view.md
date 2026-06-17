# Process View

**Input**: `.specify/memory/architecture-scenario-view.md`, `.specify/memory/architecture-logical-view.md`

**Purpose**: Derive runtime collaboration, handoffs, approvals, receipts, state advancement, and failure closure from scenario paths and logical boundaries.

## Architecture Intent

Preserve runtime collaboration semantics where fact production, fact handoff, authoritative acceptance,
and user-facing feedback remain decoupled but traceable, with explicit closure behavior for invalid
inputs and temporary delivery failures.

## Core Tensions

| Tension | Current Tradeoff Direction | Process Consequence |
|---------|----------------------------|---------------------|
| Continuous flow throughput vs strict acceptance validation | Validate before authoritative advancement while keeping runtime flow alive for later inputs | Runtime links include rejection closures that do not halt subsequent processing |
| Immediate local observability vs asynchronous downstream delivery | Emit immediate operator receipts while preserving delayed but durable handoff semantics | Participants can confirm progress locally even when downstream progression is pending |
| Autonomous participant operation vs end-to-end traceability | Keep participants independently runnable but linked through stable fact/correlation semantics | End-to-end path reconstruction relies on shared identity and consistent receipts |

## Stable Boundaries

| Boundary | Must Remain Stable Because | Explicitly Does Not Control |
|----------|----------------------------|-----------------------------|
| Detection runtime flow | Validates and emits candidate facts for platform use | Authoritative persistence outcomes |
| Asynchronous handoff flow | Separates producer and consumer lifecycle to support resilient collaboration | User-facing inventory decisions |
| Authoritative acceptance flow | Decides canonical state advancement for device lifecycle | Observation capture behavior and transport ownership |
| User interaction flow | Presents and submits intents around authoritative state | Internal delivery compensation policy |

## Change Axes

| Expected Change | Isolated By | Process Impact |
|-----------------|-------------|----------------|
| Added downstream consumers | Asynchronous handoff semantics with stable fact meaning | New runtime links attach without redefining existing link contracts |
| Evolution of validation and compensation policy | Explicit rejection and closure links at each handoff | Failure branches can refine handling while preserving continuation invariant |
| Expanded operator workflows | Dedicated user participation receipts and intent submission links | User-facing enhancements remain detached from detection and transport runtime loops |

## Invariants

| Invariant | Source Scenario / Runtime Link | Risk If Violated |
|-----------|--------------------------------|------------------|
| Each handoff preserves the same logical identity semantics | Detection-to-publication, publication-to-intake, intake-to-authoritative-state links | Cross-participant correlation breaks and outcome traceability is lost |
| Rejection paths must close explicitly without blocking later valid flow | Discovery rejection, intake rejection, management validation failure branches | Failure handling becomes implicit and can stall or corrupt runtime progression |
| Authoritative advancement only occurs after acceptance at the inventory boundary | Intake acceptance and management intent acceptance links | Canonical state can be mutated by non-authoritative participants |

## Non-goals / Anti-patterns

| Non-goal / Anti-pattern | Why It Is Out of Scope or Harmful |
|-------------------------|-----------------------------------|
| Modeling runtime collaboration as one synchronous chain | Removes resilience and independent participant operation assumptions |
| Omitting explicit closure for invalid or ambiguous inputs | Hides operational failure semantics and weakens governance evidence |
| Letting user interaction links bypass authoritative acceptance | Breaks unified validation and idempotency guarantees |

## Main Runtime Links

| Runtime Link | Trigger | Source | Target | Transferred Content / Fact | Completion Condition |
|--------------|---------|--------|--------|----------------------------|----------------------|
| RL-01 Observation Interpretation | New observation evidence arrives | Observation environment | Detection runtime participant | Candidate session/device evidence and interpretation context | Evidence is validated or rejected with explicit diagnostic outcome |
| RL-02 Detection Publication | Candidate fact is validated | Detection runtime participant | Asynchronous handoff boundary and local operator receipt boundary | Stable session/device facts with correlation semantics | Fact is emitted for downstream consumption and local observability is updated |
| RL-03 Historical Projection | Published session fact becomes eligible for retrieval | Asynchronous handoff boundary | Historical query projection participant | Session fact semantics for investigative retrieval | Fact is available for bounded retrieval or documented as pending |
| RL-04 Intake Bridging | Published device fact is consumed | Asynchronous handoff boundary | Intake validation participant | Device fact and correlation identity | Candidate is accepted for authoritative submission or rejected with closure |
| RL-05 Authoritative State Advancement | Intake candidate or management intent reaches acceptance point | Intake validation participant or user interaction participant | Authoritative inventory participant | Canonical device identity, lifecycle evidence, acceptance intent | Canonical state advances deterministically or returns explicit rejection |
| RL-06 User Inventory Interaction | User requests inventory view or submits management intent | User interaction participant | Authoritative inventory participant | Retrieval request or management intent | User receives inventory result or actionable rejection/unavailability outcome |

## Handoffs and Approvals

| Handoff / Approval | From | To | Meaning | Accepted Path | Rejected / Returned Path |
|--------------------|------|----|---------|---------------|--------------------------|
| H-01 Detection Validation Handoff | Observation interpretation | Detection publication | Candidate evidence is approved as reusable fact | Candidate becomes published fact | Candidate is rejected with diagnostics and flow continues |
| H-02 Device Fact Intake Approval | Asynchronous handoff boundary | Intake validation participant | Published device fact is checked for authoritative eligibility | Eligible candidate advances to authoritative acceptance | Candidate is closed as rejected with explicit reason |
| H-03 Canonical State Approval | Intake validation or user management intent | Authoritative inventory participant | Canonical state mutation is requested | State is accepted as consolidated canonical result | Request is rejected or returned as non-mutating outcome |
| H-04 Historical Retrieval Approval | Historical query request | Historical projection participant | Retrieval criteria are applied to projected session facts | Matching facts are returned as bounded result | Empty or delayed availability outcome is returned explicitly |

## Receipts and User Participation

| Receipt / Participation Point | Sender | Receiver | Content | User Action | Architecture Consequence |
|-------------------------------|--------|----------|---------|-------------|--------------------------|
| R-01 Detection Progress Receipt | Detection publication boundary | Network Operator | Live confirmation of session/device fact outcomes including rejections | Continue validation or investigate diagnostics | Preserves immediate operational confidence without requiring downstream systems |
| R-02 Ingestion Processing Receipt | Intake validation participant | Platform Operator | Acceptance, retry/degradation, or rejection progression for device intake | Monitor continuity and intervene on persistent failures | Makes handoff health observable without changing ownership boundaries |
| R-03 Inventory Query Receipt | Authoritative inventory participant | Device Manager | Current canonical inventory or explicit empty/unavailable outcome | Refresh, inspect, or defer action | Keeps user workflow aligned with authoritative state |
| R-04 Management Submission Receipt | Authoritative inventory participant | Device Manager | Success, idempotent success, or validation rejection for submitted intent | Adjust input or continue operations | Confirms unified acceptance policy across manual and automated paths |

## Failure, Degradation, and Closure

| Failure / Branch | Detection Boundary | Responsible Boundary | Degradation or Compensation | User-Visible Result | Closure Condition |
|------------------|--------------------|----------------------|-----------------------------|---------------------|-------------------|
| Invalid observation evidence | Observation Interpretation | Detection runtime participant | Reject candidate and continue evaluating later evidence | Operator receives diagnostic rejection without run termination | Rejection is recorded and flow resumes with next evidence |
| Temporary asynchronous delivery disruption | Detection publication or intake bridging link | Asynchronous handoff and intake validation participants | Delay advancement and apply bounded retry/degradation semantics | Platform operator sees delayed progression rather than silent drop | Fact eventually advances or is explicitly closed as unresolved |
| Identity mismatch or invalid intake candidate | Intake validation participant | Intake validation participant | Reject authoritative submission and keep canonical state unchanged | Platform operator sees explicit rejection reason | Candidate marked closed-rejected and subsequent candidates continue |
| Authoritative state temporary unavailability | Canonical state approval link | Authoritative inventory participant | Return recoverable unavailable outcome and preserve prior canonical state | Device manager sees retryable unavailable state | Availability restored and action retried with unchanged semantics |
| Retrieval with no matching results | Historical retrieval approval link | Historical query projection participant | Return explicit empty result as normal branch | Operator sees no-match outcome, not runtime failure | Query interaction completes normally |

## Process Gaps

| Gap | Affected Runtime Link / Scenario | Why It Matters |
|-----|----------------------------------|----------------|
| End-to-end escalation behavior for prolonged unresolved asynchronous disruption is unspecified | RL-02 Detection Publication, RL-04 Intake Bridging | Without explicit escalation ownership, long-lived degradation can remain operationally ambiguous |
| Cross-participant receipt correlation policy is qualitative rather than explicitly standardized | RL-01 through RL-06 | Inconsistent receipt correlation can weaken incident reconstruction and compliance review |
| Concurrent authoritative requests from manual and automated origins lack explicit ordering semantics | RL-05 Authoritative State Advancement, UC-05 | Deterministic outcome guarantees under concurrency remain partially implicit |

## Prohibited Content

Do not write call stacks, queue names, retry counts, thread/process details, endpoint sequences, workflow engine configuration, or orchestration code here.
