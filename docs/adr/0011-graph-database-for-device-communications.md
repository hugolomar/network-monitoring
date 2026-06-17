# ADR 0011: Graph Database for Device Communication Relationships

- Status: Accepted
- Date: 2026-06-17

## Context

Feature `007-device-communication-graph` introduces projection and querying of communication
relationships between device identities. The system must support:

- relationship-centric traversal (neighbors by hop depth),
- bounded subgraph/snapshot retrieval (`/api/graph/devices`, `/api/graph/devices/all`),
- relationship properties (`protocol`, `weight`, `firstSeenUtc`, `lastSeenUtc`),
- retention cleanup over stale edges and orphan external-host nodes.

These operations are graph-native and become complex/expensive when modeled as relational joins for
multi-hop traversal and relationship maintenance.

## Decision

Use a **graph database** as the persistence engine for communication projection and graph query
workloads, with **Neo4j** as the reference implementation in non-testing environments.

- Internal and external communication endpoints are represented as graph nodes.
- Communication links are represented as `COMMUNICATED_WITH` relationships with temporal/counter
  properties.
- Device inventory remains the authoritative store for internal-device lifecycle; the graph store is
  a query-optimized projection.
- In-memory graph repositories remain available for tests and optional local fallback.

## Rationale

- Graph traversal and neighborhood queries are first-class operations in this feature.
- Relationship updates and retention rules map naturally to graph edges/nodes.
- Query readability and maintainability are improved by graph-native query semantics.
- Neo4j provides mature graph tooling and query capabilities that fit bounded neighborhood/snapshot
  APIs and relationship-heavy domain semantics.

## Alternatives Considered

1. **Relational-only model (PostgreSQL joins)**
   - Pros: fewer datastore technologies.
   - Cons: multi-hop traversal and relationship-heavy queries become less natural and harder to evolve.
   - Rejected for this feature slice.

2. **Amazon Neptune (managed graph service)**
   - Pros: managed cloud graph runtime with graph-native query support.
   - Cons: introduces cloud-provider coupling and deployment assumptions outside current reference stack.
   - Rejected for this iteration.

3. **JanusGraph (distributed graph over external storage backends)**
   - Pros: scalable graph architecture for very large distributed graph workloads.
   - Cons: higher operational and integration complexity for current bounded-scope feature needs.
   - Rejected for this iteration.

4. **ArangoDB / multi-model graph-capable database**
   - Pros: flexible model choices (document + graph) in one engine.
   - Cons: graph-only semantics are less focused than a dedicated graph-first choice for this slice.
   - Rejected for this iteration.

5. **Document store projection**
   - Pros: simple ingestion model.
   - Cons: weaker native support for bounded graph traversal and relationship semantics.
   - Rejected.

6. **In-memory only**
   - Pros: very simple local setup.
   - Cons: not suitable as the durable runtime for projected communication graph behavior.
   - Rejected for non-testing environments.

## Consequences

- **Positive:** traversal-oriented APIs and retention operations are simpler and clearer to implement.
- **Negative:** introduces graph database operational overhead (deployment, credentials, monitoring).
- **Boundary note:** graph data is a projection; canonical internal-device ownership stays in inventory.
- **Testing note:** in-memory repositories remain valid for fast tests and deterministic local scenarios.
