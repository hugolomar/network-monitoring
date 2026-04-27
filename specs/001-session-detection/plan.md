# Implementation Plan: Probe Session Detection Visibility

**Branch**: `001-session-detection` | **Date**: 2026-04-20 | **Spec**: `/home/hugo/network-monitoring/specs/001-session-detection/spec.md`  
**Input**: Feature specification from `/home/hugo/network-monitoring/specs/001-session-detection/spec.md` covering **US1–US2**, **FR-001–FR-016**, **SC-001–SC-005**.

## Summary

The feature provides two capabilities over the same session detection semantics:

1. **US1**: operator-visible structured records (console / JSONL) for live validation.
2. **US2**: optional publication of validated outcomes to the platform asynchronous event stream
   (Kafka + Schema Registry, Avro per ADR 0006, topic `sessions.detected`).

Shared architecture remains clean/hexagonal: `ITrafficProvider` for capture and `IMessagePublisher`
for outputs. Kafka/Avro publishing is an infrastructure adapter alongside `ConsolePublisher`; domain
and use-case rules remain independent of transport details.

Architecture decision records for this feature: `docs/adr/0006-avro-schema-registry-for-session-kafka-payloads.md`,
`docs/adr/0007-kafka-kraft-without-zookeeper.md`, and
`docs/adr/0008-mutual-tls-for-kafka-and-service-clients.md`.

## Planned Outcomes

### US1 — Observe captured sessions live

- **Intent**: Operators validate capture and stable session shape without downstream systems.
- **Primary pieces**: `NetworkMonitoring.Probe`, tshark capture adapter, validation, `ConsolePublisher`,
  and console JSONL contract.
- **Verification**: SC-001–SC-004; xUnit unit/integration tests for capture-to-console.

### US2 — Feed the platform session event stream

- **Intent**: Publish the same validated detections on Kafka for platform consumers.
- **Primary pieces**: Kafka + Avro publisher, explicit topic `sessions.detected`, Schema Registry subject
  `sessions.detected-value`, reference Kafka stack, and TLS/mTLS configuration posture.
- **Verification**: SC-005; opt-in integration test `KafkaSessionEventPublishIntegrationTests` with
  `RUN_KAFKA_INTEGRATION=1`; manual validation in `quickstart.md`.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: .NET Worker host; shared SeedWork domain; tshark CLI; Confluent.Kafka,
Confluent.SchemaRegistry, and Avro serializers for producers  
**Storage**: Apache Kafka reference stack (3 brokers, KRaft) + Schema Registry for emitted
`SessionDetected` events  
**Testing**: xUnit unit/integration tests; opt-in Kafka integration test when the compose stack is up  
**Target Platform**: Linux host/container with capture capability; Docker Compose for Kafka + Registry validation  
**Constraints**: SeedWork immutability; invalid observations must not stop stream processing; session
identity for deduplication and Kafka record key must match; TLS minimum and mTLS for non-dev per ADR 0008;
Kafka KRaft only per ADR 0007; contract evolution must remain compatible unless explicitly versioned

## Constitution Check

- **Shared Domain Integrity**: `Session` remains a shared-domain entity inheriting `Entity`.
- **SeedWork Immutability**: Only constitution-allowed edits under `SeedWork/`.
- **Boundary Contracts**: Console JSONL and Kafka Avro value contracts are stable artifacts.
- **Security Controls**: Probe-to-Kafka transport uses encrypted communication, with mTLS target posture
  in integration/staging/production and documented dev relaxation.
- **Containerized Deployables**: Probe Dockerfile and reference Kafka stack remain reproducible.
- **Verification Path**: Unit/integration tests plus SC-005 stream sampling.

**Gate status**: PASS.

## Project Structure

```text
specs/001-session-detection/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

```text
src/
├── NetworkMonitoring.Domain/
└── NetworkMonitoring.Probe/

tests/
├── NetworkMonitoring.Probe.UnitTests/
└── NetworkMonitoring.Probe.IntegrationTests/
```

## Design Artifacts

- `data-model.md`: session entity and emission policy.
- `contracts/`: console and Kafka/Avro contracts.
- `quickstart.md`: console validation, Kafka + Registry bring-up, explicit topic provisioning, and
  stream publication validation.

## Next Step (Spec Kit)

Follow `specs/001-session-detection/tasks.md` for implementation phases covering console visibility,
Kafka publication, topic provisioning, mTLS posture, and SC-005 verification.
