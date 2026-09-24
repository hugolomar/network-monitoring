# Implementation Plan: Production Observability

**Branch**: `008-observability` | **Date**: 2026-07-07 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/008-observability/spec.md`

## Summary

Establish a cross-cutting production observability capability that standardizes correlation, structured
logging, service metrics, distributed tracing, health signaling, and actionable alerts across
production-path services **and the platform components they run on**, adds business-flow throughput and
capture-loss visibility so pipeline limitations are measurable, and provides log-to-trace navigation in a
single exploration UI, with objective verification gates in tests/CI and alignment with the observability
stack defined in ADR 0013.

## Architecture Grounding

**Architecture files read**:
- `.specify/memory/architecture.md`
- `.specify/memory/architecture-scenario-view.md`
- `.specify/memory/architecture-logical-view.md`
- `.specify/memory/architecture-process-view.md`
- `.specify/memory/architecture-development-view.md`
- `.specify/memory/architecture-physical-view.md`

**Applicable stable boundaries and forbidden crossings**:
- Shared-domain semantics remain authoritative; observability is cross-cutting and MUST NOT redefine
  session/device business meaning.
- Deployable unit independence is preserved: Probe, Integration Console, Backend, and Frontend retain
  separate runtime ownership while emitting compatible telemetry.
- Asynchronous backbone remains a collaboration boundary, not a source-of-truth boundary; observability
  signals must reflect closure/failure without altering ownership semantics.
- Interaction boundaries (UI/API) continue consuming contracts; observability additions cannot create
  direct UI-to-persistence coupling.

**Architecture constraints and unresolved gaps that bound this plan**:
- Physical-view gap on cross-environment observability normalization is currently qualitative; this plan
  introduces explicit shared signal requirements to reduce drift.
- Outage escalation policy for prolonged asynchronous disruption remains broader than this slice;
  observability scope includes detection/diagnosis signals, not new global escalation policy.
- Security hardening beyond telemetry hygiene is deferred; this slice enforces no-PII/no-secret
  telemetry output but does not redefine all security controls.

**Conflicts and resolution path**:
- No blocking conflict found between feature direction and architecture SSOT.
- If future platform constraints require non-OTel telemetry pipelines, resolve via ADR supersession and
  spec update while preserving constitutional observability obligations.

**Architecture drift risks to watch during implementation**:
- Service-local, incompatible telemetry conventions that break cross-service correlation.
- Over-coupling instrumentation code to one backend vendor/tool.
- Treating observability endpoints as optional and bypassing CI quality gates.

## Technical Context

**Language/Version**: C# / .NET 10 (Probe, Integration Console, Backend), TypeScript/React (Frontend)  
**Primary Dependencies**: OpenTelemetry SDK/runtime instrumentation, OpenTelemetry Collector (application
telemetry ingest, platform metric receivers), Prometheus, Alertmanager, Grafana, Elastic APM Server,
Fluent Bit (platform log entry point and multi-line assembly), Logstash (platform log normalization),
Elasticsearch, Kibana  
**Storage**: Prometheus TSDB for metrics; Elasticsearch for centralized log indexing/search and for APM
trace data, which is what enables log-to-trace navigation in one store; existing operational datastores
remain authoritative for domain data (PostgreSQL/Neo4j/Kafka)  
**Testing**: xUnit (unit/integration), frontend Vitest where relevant, CI smoke/contract checks for
observability gates  
**Target Platform**: Linux containerized local/reference stack and CI  
**Project Type**: Cross-cutting backend/probe/integration/frontend capability plus infra/docs contracts  
**Performance Goals**: Meet spec SC-001..SC-014 (traceability coverage, diagnosis time, critical-flow
signal coverage, proactive alerting, zero sensitive telemetry leakage, centralized log retrieval time,
platform coverage, log-to-trace navigation time, parse-failure visibility, alert delivery and
suppression, pipeline throughput comparability, loss-cause attribution, end-to-end freshness, browser
segments in traces)  
**Constraints**: Platform-agnostic required behaviors (no vendor lock-in at requirement level), no PII in
telemetry, preserve clean/hexagonal boundaries, enforce verifiable gates. ADR 0013 defines the
reference stack for this increment and does not alter vendor-neutral requirement semantics.  
**Scale/Scope**: Apply the capability to all production-path services in the current repository and to
the platform components of the reference stack (broker, connector runtime, relational store, graph
store); define the shared signal taxonomy, the log field contract, and the validation path

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Articles 1-2, 14-15 (framework and clean dependency direction): PASS**  
  Instrumentation will be introduced through boundary-friendly adapters and host wiring without
  inverting dependencies.
- **Articles 3-5, 21 (shared domain authority + SeedWork immutability): PASS**  
  No SeedWork modification required; observability concerns remain outside domain authority internals.
- **Articles 6-7 (contract-first boundaries): PASS**  
  Any telemetry-related API/event contract additions are additive and documented in feature artifacts.
- **Articles 8-9 (security baseline): PASS**  
  Telemetry hygiene includes explicit prohibition of secrets/PII; authN/Z behavior remains intact.
- **Articles 10-11 (objective verification): PASS**  
  Plan includes testable observability gates in CI and integration validation.
- **Articles 23-24 (incremental and proportional modularity): PASS**  
  Capability is delivered incrementally across runtime units with bounded scope.
- **Articles 29-31 (documentation standards): PASS**  
  Public contracts and relevant tests/docs will be updated as part of this feature slice.
- **Articles 32-36 (observability obligations): PASS**  
  This feature directly implements the constitutional observability principles with measurable outcomes.

## Phase 0 - Research

Research outcomes are consolidated in [research.md](./research.md):
- telemetry signal taxonomy and cardinality policy (including browser and pipeline stages),
- correlation/trace propagation strategy across sync + async + browser boundaries,
- health endpoint and readiness semantics across services,
- alerting trigger model for critical-flow degradation with Alertmanager delivery,
- objective verification patterns for observability gates in CI,
- platform log path (runtime driver → Fluent Bit → Logstash) with measured flat payload shapes,
- multi-line record joining, event-time extraction, and durable buffering responsibilities per hop,
- business-flow metric taxonomy per pipeline stage, including capture-loss attribution,
- platform metric receivers including `docker_stats` and search-store rejection series.

All initial technical unknowns for this feature slice are resolved in research artifacts.

## Phase 1 - Design Artifacts

- [data-model.md](./data-model.md): observability entities/signals and validation invariants.
- [contracts/observability.md](./contracts/observability.md): cross-service observability behavior
  contract and required fields.
- [quickstart.md](./quickstart.md): local validation flow for observability behavior and gates.

## Project Structure

### Documentation (this feature)

```text
specs/008-observability/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── observability.md
│   └── critical-flow-inventory.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── NetworkMonitoring.Probe/
├── NetworkMonitoring.IntegrationConsole/
├── NetworkMonitoring.Backend/
└── NetworkMonitoring.Frontend/

tests/
├── NetworkMonitoring.Probe.UnitTests/
├── NetworkMonitoring.Backend.UnitTests/
├── NetworkMonitoring.Backend.IntegrationTests/
└── NetworkMonitoring.IntegrationConsole.UnitTests/

infrastructure/
├── observability/
├── ci/check-observability.sh
└── documentation/
```

**Structure Decision**: Implement observability as a cross-cutting capability that touches each runtime
unit through its existing boundaries. Shared telemetry semantics are documented under the feature
contracts, while service-specific implementation remains local to each deployable unit.

## Post-Design Constitution Re-check

All relevant constitutional gates remain **PASS** after design artifact generation:

- Verifiable quality obligations are represented as explicit acceptance and validation paths.
- Security-by-default is preserved via telemetry hygiene constraints.
- Contract-first and incremental delivery constraints remain intact.
- New observability principles (Articles 32-36) are mapped directly to feature artifacts and planned
  verification.

## Complexity Tracking

No constitutional violations identified for this planning pass.

## Implementation Compliance Record

- **Validation date**: 2026-07-25 (full scope, FR-001..FR-019 / SC-001..SC-014)
- **Result**: PASS
- **Scope delivered**: application telemetry, platform logs and metrics, Elastic APM traces, Alertmanager
  delivery, pipeline business metrics, browser OTLP telemetry, Grafana triage/service/pipeline
  dashboards, and documentation aligned to ADR 0013.
- **Evidence anchors**:
  - Observability CI gate script in `infrastructure/ci/check-observability.sh`
  - SeedWork immutability gate in `infrastructure/ci/check-seedwork-immutability.sh`
  - Observability verification workflow in `specs/008-observability/quickstart.md`
  - Cross-service contract in `specs/008-observability/contracts/observability.md`
  - Critical-flow inventory for SC-003 in `specs/008-observability/contracts/critical-flow-inventory.md`
  - Definitive stack decision in `docs/adr/0013-definitive-observability-stack.md`
