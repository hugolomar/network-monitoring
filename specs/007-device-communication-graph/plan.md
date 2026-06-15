# Implementation Plan: Device Communication Graph

**Branch**: `007-device-communication-graph` | **Date**: 2026-05-21 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/007-device-communication-graph/spec.md`

## Summary

Deliver a bounded, authenticated communication-graph capability that projects enriched session facts
into graph relationships, exposes `GET /api/graph/devices` for neighborhood traversal, executes
scheduled retention cleanup without impacting existing non-graph backend behavior, and adds a
frontend graph exploration view for operators.

## Architecture Grounding

**Architecture files read**:
- `.specify/memory/architecture.md`
- `.specify/memory/architecture-scenario-view.md`
- `.specify/memory/architecture-logical-view.md`
- `.specify/memory/architecture-process-view.md`
- `.specify/memory/architecture-development-view.md`
- `.specify/memory/architecture-physical-view.md`

**Applicable stable boundaries and forbidden crossings**:
- Shared domain semantic authority remains in `NetworkMonitoring.Domain/SeedWork`; graph projection
  is a bounded read/projection slice and must not redefine device/session semantics.
- Inventory remains canonical for internal-device lifecycle; graph projection cannot become authority.
- Asynchronous collaboration and graph storage are collaboration/projection boundaries, not truth
  authority; non-graph endpoints must stay insulated from graph outages.
- User interaction/API boundary must consume authoritative outcomes and not directly couple to capture
  or transport internals.

**Constraints and unresolved architecture gaps that bound this plan**:
- Authorization semantics are intentionally broad in this slice (authenticated access) and role-hardening
  remains deferred.
- Cross-participant outage escalation policy is still qualitative; this plan enforces explicit closure
  semantics locally (503 + diagnostics) without introducing new global escalation mechanics.
- Concurrency ordering between manual/automated inventory updates remains outside this feature scope.

**Conflicts and resolution path**:
- No blocking conflict identified between feature direction and architecture SSOT.
- If future hardening narrows auth scope or introduces contract-breaking changes, resolve by updating
  architecture SSOT and re-planning affected slices before implementation expansion.

**Architecture drift risks to watch during implementation**:
- Introducing graph data as canonical inventory source.
- Coupling non-graph endpoint health to graph-store availability.
- Bypassing contract-first behavior for graph query/error payload evolution.

## Technical Context

**Language/Version**: C# / .NET 10 for API/hosted services; JSON for connector configuration  
**Primary Dependencies**: ASP.NET Core minimal APIs, Microsoft DI/options hosting stack, graph database adapter boundary, Kafka Connect Neo4j sink contract  
**Storage**: Graph database for communication projection; existing inventory store remains authoritative for internal devices  
**Testing**: xUnit integration tests with backend test host and graph-focused API/integration coverage  
**Target Platform**: Linux containerized runtime in local/CI environments  
**Project Type**: Backend web-service extension plus frontend UI extension plus connector/config contract artifacts  
**Performance Goals**: SC-001 median projection latency <= 2s; bounded retrieval responsive within configured depth/limit caps  
**Constraints**: Auth required for graph endpoint; scheduled-only 24h retention; bounded projection retries; structured logs + counters/latency metrics; graph outage isolation from existing endpoint families  
**Scale/Scope**: One graph endpoint, projection + retention lifecycle, graph UI exploration page, connector baseline, and validation artifacts for FR-001..FR-022 / SC-001..SC-007

### Runtime implementation note (2026-06-15)

- Maintainer confirmed migration of graph runtime behavior from in-memory projection to Neo4j-backed
  persistence for non-testing environments.
- `InMemoryGraphStore` remains available for `Testing` and optional explicit `Provider=InMemory` runs.
- Neo4j is now part of the local reference stack and the backend defaults to `Provider=Neo4j`.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Article 1 / Framework Baseline**: PASS (`.NET 10` remains baseline).
- **Articles 2, 14, 15 / Clean boundaries and inward dependency**: PASS (graph boundary added under backend application/infrastructure/host layering).
- **Articles 3-5 / Shared domain authority + SeedWork immutability**: PASS (no planned SeedWork changes; shared semantics consumed, not redefined).
- **Articles 6-7 / Contract-first stability**: PASS (graph contract is additive; explicit compatibility/error payload rules maintained).
- **Articles 8-9 / Security controls**: PASS for this slice (authenticated access mandatory; role-based
  authorization enforced for graph reads with allowed roles `admin`, `analyst`, `auditor`, `integration`;
  fine-grained role filtering deferred and documented).
- **Articles 10-11 / Verifiable quality gates**: PASS (integration/contract validation paths and quickstart evidence included).
- **Articles 17, 23, 24 / Unified validation + incremental compatibility + proportional modularity**: PASS (additive module, isolated blast radius, bounded structure).
- **Articles 29-31 / Documentation standards**: PASS (public/backend boundary docs and rationale comments required in task plan).

## Phase 0 - Research

Research outcomes are consolidated in [research.md](./research.md):
- Graph edge identity and idempotent upsert semantics.
- External-host representation and cleanup ownership.
- Retrieval contract shape with depth/limit bounding.
- Scheduled retention model and outage-isolation behavior.
- Auth scope, retry policy, and observability baseline.

All prior NEEDS CLARIFICATION items are resolved.

## Phase 1 - Design Artifacts

- [data-model.md](./data-model.md): graph entities, invariants, lifecycle transitions.
- [contracts/graph-api.md](./contracts/graph-api.md): endpoint contract, access policy, response/error semantics.
- [quickstart.md](./quickstart.md): reproducible validation flow for projection/query/retention/outage/auth/observability.

## Project Structure

### Documentation (this feature)

```text
specs/007-device-communication-graph/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── graph-api.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── NetworkMonitoring.Backend/
│   ├── Host/Endpoints/
│   ├── Host/Services/
│   ├── Application/UseCases/
│   ├── Application/Ports/
│   └── Infrastructure/Graph/
└── ...

infrastructure/connectors/configs/
└── neo4j-sink-sessions-enriched.json

tests/
└── NetworkMonitoring.Backend.IntegrationTests/
```

**Structure Decision**: Extend the existing backend slice using the same clean layering and keep all
graph concerns isolated under `Infrastructure/Graph` + `Application` + `Host` boundaries. This preserves
incremental compatibility with existing modules and aligns with architecture SSOT ownership rules.

## Post-Design Constitution Re-check

All relevant constitutional gates remain **PASS** after design artifact generation:

- Contract evolution remains additive and documented (Articles 6-7).
- Shared domain authority and SeedWork immutability constraints remain respected (Articles 3-5, 21).
- Security, verification, and documentation obligations are explicitly represented in artifacts and tasks
  (Articles 9-11, 29-31).
- Incremental isolation and compatibility obligations are preserved for non-graph endpoint families
  (Articles 23-24).

## Complexity Tracking

No constitutional violations identified for this planning pass.
