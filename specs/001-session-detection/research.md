# Research: Probe Session Detection Visibility

## Decision 1: Runtime and project shape
- **Decision**: Implement the probe as a .NET worker-style console application with
  clean architecture layering (Domain, Application, Infrastructure, Host) inside a single
  module project.
- **Rationale**: The constitution sets .NET as mandatory baseline and requires
  clean/hexagonal separation with dependency inversion.
- **Alternatives considered**:
  - Plain single-layer console app: faster start but violates architectural boundaries.
  - Python wrapper first: acceptable in general architecture, but conflicts with current
    constitutional baseline for this repository.

## Decision 2: Capture adapter approach
- **Decision**: Introduce a capture input port in Application (interface example:
  `ITrafficProvider`) and implement it in Infrastructure with `TsharkTrafficProvider`, which
  reads line-buffered packet metadata and maps records into shared domain session entities.
- **Rationale**: This aligns with the architecture document and keeps capture mechanics
  isolated from domain rules while making provider replacement explicit.
- **Alternatives considered**:
  - Native packet parsing in-process: more control but significantly higher complexity.
  - File-based PCAP batch processing only: does not satisfy live validation intent.

## Decision 3: Output port abstraction for console and Kafka
- **Decision**: Define an output port abstraction in Application (`IMessagePublisher`) with
  `ConsolePublisher` for operator-visible output and a **Kafka + Avro** adapter for stream
  publication (US2), composed so use-case logic stays single-path.
- **Rationale**: Dependency inversion; same validated `Session` drives both outputs per spec.
- **Alternatives considered**:
  - Write directly to console from use cases: blocks extensibility.
  - Kafka-only without console: weakens operator validation (US1).

## Decision 4: Emission format for console validation
- **Decision**: Emit structured JSON lines for `SessionDetected` records to console.
- **Rationale**: Human-readable enough for validation and close to future event payload
  usage; simplifies parity checks before Kafka.
- **Alternatives considered**:
  - Free-form text logs: easy to read but brittle for automated validation.
  - Binary serialization: unnecessary for this phase.

## Decision 5: Validation and malformed capture handling
- **Decision**: Apply explicit validation pattern before emission; invalid/partial records are
  discarded with aggregated validation diagnostics while processing continues. Exceptions are
  reserved for unexpected runtime failures.
- **Rationale**: Matches feature requirements for resilience and stable structures.
- **Alternatives considered**:
  - Fail-fast on first malformed record: too disruptive for continuous monitoring.
  - Emit partially valid entities: risks downstream contract drift.

## Decision 6: Test strategy for first increment
- **Decision**: Use xUnit with unit tests for mapping/validation and integration tests for
  end-to-end capture-to-console flow using fixture input streams.
- **Rationale**: Provides objective verification path required by the constitution.
- **Alternatives considered**:
  - Manual-only verification: insufficient for repeatability.
  - Full system tests with real interfaces only: slower and less deterministic for early phase.

## Decision 7: Entity identity type
- **Decision**: Keep identifier ownership centralized in SeedWork `Entity`, and have probe/domain
  entities inherit identifier semantics without local overrides.
- **Rationale**: Preserves shared-domain consistency while allowing identifier-type evolution to be
  governed explicitly by ADRs.
- **Alternatives considered**:
  - Probe-local identity type overrides: rejected to avoid contract drift against shared domain.
  - Dual identifier model (internal + external): deferred due to added complexity at this stage.

## Decision 8: Network primitives as ValueObjects
- **Decision**: Model `IpAddress`, `Port`, and `ProtocolType` as ValueObjects inheriting from
  SeedWork `ValueObject`.
- **Rationale**: Centralizes normalization/validation rules and prevents primitive obsession in
  domain entities.
- **Alternatives considered**:
  - Plain primitive fields (`string`/`int`) with ad-hoc validation: faster initially but easier to
    break and duplicate across entities/use cases.

## Decision 9: Next ValueObject candidates
- **Decision**: Keep additional network metadata (for example `Hostname`, `VlanId`, and
  `NetworkInterfaceName`) as optional next-iteration ValueObject candidates rather than mandatory
  in this increment.
- **Rationale**: Current phase scope focuses on capture validation and output interchangeability;
  making these mandatory now may slow initial delivery without immediate business impact.

## Decision 10: Incremental and proportional architecture
- **Decision**: Treat the probe as one incremental module of the larger system and avoid applying
  heavyweight decomposition by default in future small modules, while keeping reusable entities and
  ValueObjects in shared domain.
- **Rationale**: Keeps delivery speed high while preserving architectural discipline through ports,
  ValueObjects, and clear boundaries.
- **Alternatives considered**:
  - Enforce full multi-project split for every module: consistent but potentially over-engineered
    for small utilities.

## Decision 11: Kafka metadata mode (KRaft)
- **Decision**: New reference clusters use **KRaft only** (no ZooKeeper), typically **three brokers**
  in combined broker/controller roles for early environments unless operations later split roles.
- **Rationale**: Operational simplicity and alignment with `docs/adr/0007-kafka-kraft-without-zookeeper.md`.
- **Alternatives considered**: ZooKeeper-backed Kafka — rejected for greenfield stacks.

## Decision 12: Session event value encoding
- **Decision**: Kafka **values** for `SessionDetected` use **Avro** (Confluent wire format) with
  **Schema Registry**; canonical schema `contracts/session-detected-value.avsc`; subject
  **`sessions.detected-value`**; default topic **`sessions.detected`**.
- **Rationale**: Efficiency at scale and governed evolution per
  `docs/adr/0006-avro-schema-registry-for-session-kafka-payloads.md`.
- **Alternatives considered**: JSON on wire for all environments — rejected as default for session
  telemetry volume.

## Decision 13: Transport security to Kafka
- **Decision**: **mTLS** for probe (and other first-party clients) to Kafka in integration, staging,
  and production; dev may relax per documented non-production exception.
- **Rationale**: `docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md` and constitution
  Article 8.
- **Alternatives considered**: TLS with server auth only for production — rejected as target posture.

## Decision 14: Local stack delivery
- **Decision**: Provide **Docker Compose** (or equivalent) in-repo for **Kafka (KRaft) + Schema
  Registry** suitable for manual and automated validation; exact image lineage (Apache Kafka Docker
  vs Confluent Community) chosen during implementation to match client libraries and listener
  conventions.
- **Rationale**: Reproducible SC-005 and developer onboarding; resolves NEEDS CLARIFICATION on “which
  image” without blocking plan approval.
- **Alternatives considered**: Require external managed Kafka only — slows local iteration.

## Decision 15: Publisher composition
- **Decision**: Keep **`IMessagePublisher`** as the application port; implement **Kafka Avro**
  publishing in Infrastructure, composed with or alongside **`ConsolePublisher`** (composite or
  dual registration) so use-case code stays single-path.
- **Rationale**: Preserves hexagonal boundaries; satisfies US1 + US2.
- **Alternatives considered**: Separate use case for Kafka — duplicates orchestration and drift risk.

## Decision 16: Reference stack image lineage (US2)
- **Decision**: Local reference stack uses **Confluent Platform 7.6.1** images:
  `confluentinc/cp-kafka:7.6.1` (KRaft, three brokers) and `confluentinc/cp-schema-registry:7.6.1`,
  defined in `docker-compose.reference-stack.yml`.
- **Rationale**: Matches **Confluent .NET** client and Schema Registry Serdes versions used by the
  probe; predictable listener and tooling behavior for developers.
- **Alternatives considered**: Apache Kafka JVM-only images without Registry — rejected for this
  feature because Avro + Registry is mandatory per ADR 0006.

## Decision 17: .NET client library versions for Kafka/Avro
- **Decision**: Probe references **Confluent.Kafka**, **Confluent.SchemaRegistry**, and
  **Confluent.SchemaRegistry.Serdes.Avro** at **2.10.1**, and **Apache.Avro** at **1.12.0**
  (`NetworkMonitoring.Probe.csproj`).
- **Rationale**: Supported combination for `GenericRecord` + Schema Registry serialization; Avro C#
  API uses namespaces `Avro` / `Avro.Generic` and `GenericRecord.Add` for field values (not legacy
  `Apache.Avro` / `Put`).
- **Alternatives considered**: Older Confluent 1.x clients — rejected to stay on maintained 2.x.

## Execution Results
- `dotnet test /home/hugo/network-monitoring/src/NetworkMonitoring.Probe.sln` (2026-04-20): **all
  tests green**; unit suite **24** passed; integration suite **2** passed and **1** skipped
  (`KafkaSessionEventPublishIntegrationTests`, requires `RUN_KAFKA_INTEGRATION=1` and the Kafka compose
  stack).
- `dotnet test /home/hugo/network-monitoring/src/NetworkMonitoring.Probe.sln` (2026-04-22, after Kafka publication artifacts): **same** — **24** unit passed; integration **2** passed, **1** skipped (Kafka
  integration opt-in unchanged).
- Startup smoke run (`timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`)
  confirmed worker startup and graceful shutdown (US1 path).

## SC-005 (manual sampling checklist)
- **Intent**: With publication enabled, consume **`sessions.detected`** and verify sampled messages
  match **`session-detected-value.avsc`** semantics (field presence, types, and session identity
  consistency).
- **Recorded outcome (automation)**: The opt-in integration test
  `KafkaSessionEventPublishIntegrationTests.PublishSessionDetected_ProducesConsumableAvroValue` exercises
  produce + consume + field assertions when `RUN_KAFKA_INTEGRATION=1`.
- **Recorded outcome (manual 100% sampling)**: Not run in CI; operators should follow
  `quickstart.md` with a real consumer and attach evidence to release checklists when required.
