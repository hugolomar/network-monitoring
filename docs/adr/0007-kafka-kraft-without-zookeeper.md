# ADR 0007: Kafka Cluster Metadata with KRaft (No ZooKeeper)

- Status: Accepted
- Date: 2026-04-20

## Context

We are standing up an Apache Kafka cluster for the network-monitoring platform (default topic
`sessions.detected` for session events, with more topics to follow). The reference architecture
targets **multiple brokers** for availability (e.g. three brokers in initial deployments).

Historically, Kafka depended on **Apache ZooKeeper** for cluster metadata (controller election,
broker/topology state). Modern Kafka supports **KRaft** (Kafka Raft): metadata is managed **inside
Kafka** via a Raft-based control plane, **without** a ZooKeeper ensemble.

We must choose the metadata mode for **new** infrastructure; existing industry guidance favors
KRaft for greenfield clusters on supported Kafka versions.

## Decision

Use **KRaft-only** Kafka deployments: **do not introduce ZooKeeper** for new clusters in this
project.

- Initial sizing assumption: **three Kafka brokers** (combined broker + controller roles acceptable
  for early environments unless operations later require dedicated controllers).
- Bootstrap, monitoring, and runbooks MUST assume **KRaft** configuration (process roles, controller
  quorum voters, cluster IDs) as defined by the chosen distribution (e.g. Apache Kafka in Docker or
  Kubernetes manifests).

## Rationale

- **Operational simplicity:** One less distributed system to deploy, upgrade, secure, and back up
  (no parallel ZooKeeper lifecycle).
- **Product direction:** KRaft is the supported path for new clusters; ZooKeeper mode is legacy for
  many operators and will eventually be unnecessary for Kafka itself.
- **Fit for purpose:** A three-broker KRaft cluster matches our **availability** goal without
  adding a separate quorum stack for metadata.

## Alternatives Considered

1. **Kafka with ZooKeeper**
   - **Pros:** Familiar to teams with very old runbooks; some edge tooling still assumes ZK.
   - **Cons:** Extra moving parts, dual upgrades, wider failure domain; not justified for a **new**
     cluster in this codebase.
   - **Rejected.**

2. **Managed Kafka only (cloud)**
   - **Pros:** Provider hides KRaft vs ZK entirely.
   - **Cons:** Not assumed for all dev/staging workflows; local and self-hosted stacks still need an
     explicit decision.
   - **Out of scope** as the universal choice, but compatible with this ADR when the provider is
     KRaft-based under the hood.

## Consequences

- **Positive:** Simpler reference topology (Kafka + Schema Registry + clients); aligns with current
  Kafka deployment practice.
- **Negative:** Team must learn **KRaft-specific** bootstrap (cluster id, controller quorum, storage
  layout) for the chosen images/charts; migration from hypothetical legacy ZK clusters is **not** a
  current requirement.
- **Follow-up:** Document concrete **Docker Compose / Helm** settings in infrastructure docs or
  compose files; ensure health checks and persistent volumes satisfy KRaft controller requirements.
