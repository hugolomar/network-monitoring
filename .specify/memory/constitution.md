<!--
Sync Impact Report
- Version change: 1.2.1 -> 1.3.0
- Modified principles:
  - Template Principle 1 -> I. Clean/Hexagonal Shared Domain Core
  - Template Principle 2 -> II. Contract-First Event and API Boundaries
  - Template Principle 3 -> III. Security-by-Default for Service and User Access
  - Template Principle 4 -> IV. Verifiable Quality Gates
  - Template Principle 5 -> V. Evolution Through Explicit Trade-Offs
- Added sections:
  - Architecture and Domain Constraints
  - Delivery Workflow and Review Expectations
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ updated: .specify/templates/plan-template.md
  - ✅ reviewed, no change required: .specify/templates/spec-template.md
  - ✅ reviewed, no change required: .specify/templates/tasks-template.md
  - ✅ reviewed, no change required: .specify/templates/checklist-template.md
  - ✅ reviewed, no change required: .specify/templates/agent-file-template.md
  - ⚠ pending (path not present in repository): .specify/templates/commands/*.md
- Follow-up TODOs:
  - None
-->
# Network Monitoring Constitution

## Core Principles

### I. Clean/Hexagonal Shared Domain Core
**Article 1 — Framework Baseline.** The project framework baseline MUST be .NET 10.

**Article 2 — Architectural Style.** The architecture MUST follow clean/hexagonal boundaries,
with domain rules isolated from infrastructure details.

**Article 3 — Shared Domain Authority.** The shared domain model is authoritative and MUST be
based on abstractions in `src/NetworkMonitoring.Domain/SeedWork`.

**Article 4 — Entity Lineage.** The domain entities defined by this project scope (`Session` and
`Device`) MUST belong to the shared domain and MUST inherit from `Entity` in
`src/NetworkMonitoring.Domain/SeedWork/Entity.cs`. `Device` MUST implement `IAggregateRoot` from
`src/NetworkMonitoring.Domain/SeedWork/IAggregateRoot.cs`.

**Article 5 — SeedWork Immutability.** Files under `src/NetworkMonitoring.Domain/SeedWork` MUST
NOT be modified, except `src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj`
and `src/NetworkMonitoring.Domain/SeedWork/GlobalUsings.cs` when strictly required for build or
wiring updates.

Rationale: these rules preserve a stable shared domain contract across probe, backend, and
integration components while enforcing a consistent aggregate model.

### II. Contract-First Event and API Boundaries
**Article 6 — Contract Stability.** Kafka topics and HTTP endpoints MUST be treated as stable
contracts.

**Article 7 — Change Discipline.** Changes to payloads, schemas, or endpoint behavior MUST be
explicit, documented, and backward-compatible unless an approved breaking change is declared.

Rationale: network monitoring pipelines are event-driven and multi-consumer; contract drift
causes cross-service regressions.

### III. Security-by-Default for Service and User Access
**Article 8 — Service Security.** Service-to-service communication MUST enforce TLS/mTLS where
supported by the target environment.

**Article 9 — User Access Control.** User-facing API access MUST be authenticated and authorized
with role-based controls aligned with the role model (for example: admin, analyst, auditor,
integration).

Rationale: the system operates in security-sensitive environments where trust boundaries are
part of core functionality.

### IV. Verifiable Quality Gates
**Article 10 — Testable Requirements.** Every feature MUST define testable acceptance criteria.

**Article 11 — Objective Verification.** Every feature MUST include at least one objective
verification path (automated test, contract test, or reproducible manual validation) before
completion.

Rationale: architectural intent only matters if behavior can be proven and regressions can be
detected early.

### V. Evolution Through Explicit Trade-Offs
**Article 12 — Decision Records.** Material architectural decisions (storage, messaging,
integration, identity, coupling) MUST be captured with rationale, alternatives, and consequences.

**Article 13 — Simplicity Bias.** Simplicity MUST be preferred unless a measurable requirement
justifies added complexity.

Rationale: explicit trade-offs improve long-term maintainability and enable consistent decisions
across iterations.

## Architecture and Domain Constraints

- **Article 14 — Domain Ownership.** Domain model ownership resides in the shared domain package.
- **Article 15 — Dependency Direction.** Application and infrastructure layers MUST depend inward
  toward domain abstractions.
- **Article 16 — Persistence Isolation.** Direct coupling from UI or integration tools to
  persistence internals is prohibited.
- **Article 17 — Unified Device Validation.** Event ingestion and manual API flows for device
  creation MUST pass through the same backend business validation path.
- **Article 18 — Containerized Deployables.** Each deployable unit MUST provide and maintain a
  Docker image definition for consistent packaging and runtime behavior across environments. The
  probe/sensor is a deployable unit and MUST be containerized.

## Delivery Workflow and Review Expectations

- **Article 19 — Planning Gate.** Plans MUST include a constitution check before implementation
  starts.
- **Article 20 — Specification Fidelity.** Specifications MUST describe shared domain entities and
  inheritance rules when entities are introduced or changed.
- **Article 21 — Task Compliance.** Tasks MUST include explicit checks that SeedWork immutability
  constraints are respected.
- **Article 22 — Review Obligation.** Pull request reviews MUST confirm compliance with all
  constitutional principles, not only code style or syntax.
- **Article 23 — Incremental Construction.** The system MUST be delivered incrementally by
  modules/slices, where each increment has bounded scope and a runnable validation path. Any change
  that may alter behavior, contracts, or assumptions of previously delivered modules MUST require
  explicit maintainer confirmation before implementation.
- **Article 24 — Proportional Modularity.** Internal structure for each module MUST be proportional
  to module complexity. Small utility modules SHOULD prefer folder-level layering; multi-project
  decomposition MUST be justified in plan/research artifacts.

## Governance

**Article 25 — Supremacy.** This constitution is the highest-priority engineering policy for this
repository. In case of conflict, this document takes precedence over local conventions.

**Article 26 — Amendment Procedure.**
- Propose the change with affected principles/sections and migration impact.
- Obtain explicit maintainer approval.
- Update dependent templates and guidance artifacts in the same change set.

**Article 27 — Versioning Policy (Semantic).**
- MAJOR: incompatible governance changes or principle removals/redefinitions.
- MINOR: new principle or materially expanded mandatory guidance.
- PATCH: clarifications, wording improvements, and non-semantic refinements.

**Article 28 — Compliance Review.**
- Each planning artifact MUST include a constitution compliance check.
- Each implementation review MUST record pass/fail against relevant principles.
- Non-compliance MUST be resolved before merge or explicitly waived with rationale.

**Version**: 1.3.0 | **Ratified**: 2026-04-03 | **Last Amended**: 2026-04-06
