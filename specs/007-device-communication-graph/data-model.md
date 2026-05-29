# Data Model: Device Communication Graph

## Overview

This feature introduces a projection model for communication relationships derived from enriched session
facts. The projection is query-oriented and does not replace authoritative device inventory ownership.

## Entities

### 1) Enriched Session Event (input fact)

Represents one validated session observation supplied by prior session-processing features.

**Relevant fields**
- Source identity
- Destination identity or destination network evidence
- Protocol
- Observation timestamp

**Validation rules**
- Source identity is required for projection.
- Protocol is required for relationship identity.
- Destination must resolve either as an internal identity or external-host evidence.

### 2) Communication Node

Represents a graph endpoint for communication traversal.

**Fields**
- `identity` (stable key in graph context)
- `nodeKind` (`InternalDevice` or `ExternalHost`)

**Rules**
- Internal devices and external hosts are distinct categories.
- Internal device lifecycle authority remains in inventory, not this projection.

### 3) Communication Relationship

Directed relationship between source and destination communication nodes.

**Identity**
- `(sourceIdentity, destinationIdentity, protocol)`

**Fields**
- `protocol`
- `weight` (count of observations mapped to this identity)
- `firstSeenUtc`
- `lastSeenUtc`

**Validation rules**
- `weight` is positive and monotonically non-decreasing.
- `lastSeenUtc >= firstSeenUtc`.
- Replays must not create duplicate relationship structure.

### 4) Graph Query Request

Caller intent for neighborhood retrieval.

**Fields**
- `rootIdentity` (required)
- `depth` (optional; capped by configuration)
- `limit` (optional; bounded by configuration)

**Validation rules**
- Root identity must be present.
- Requested depth above cap is clamped.
- Requested limit above cap is clamped.

### 5) Graph Query Result

Bounded graph response for visualization consumers.

**Fields**
- `nodes` (collection of communication nodes)
- `edges` (collection of communication relationships among returned nodes)
- `truncated` (boolean indicating size-bound truncation)

**Validation rules**
- Number of returned nodes must not exceed effective limit.
- `truncated = true` when result is cut by limit.

### 6) Retention Sweep Outcome

Result of scheduled graph hygiene execution.

**Fields**
- `staleRelationshipsRemoved`
- `orphanExternalHostsRemoved`
- `executedAtUtc`

**Validation rules**
- Stale relationship criterion is `lastSeenUtc < now - 90 days`.
- Only orphan external hosts are eligible for node cleanup.
- Internal device nodes are not removed by this sweep.

## Relationships

- Enriched Session Event -> upserts one Communication Relationship between two Communication Nodes.
- Communication Relationship references exactly one source node and one destination node.
- Graph Query Result materializes a bounded subgraph around one root node.
- Retention Sweep Outcome summarizes cleanup over Communication Relationship and Communication Node sets.

## State Transitions

### Communication Relationship lifecycle

1. **Absent** -> **Created** when first matching event is projected.
2. **Created/Updated** -> **Updated** on subsequent matching events (`weight` increments,
   `lastSeenUtc` advances, `firstSeenUtc` unchanged).
3. **Updated** -> **Removed** when relationship falls outside retention window.

### External host node lifecycle

1. **Absent** -> **Present** when first unresolved destination communication is projected.
2. **Present** -> **Present** while at least one inbound relationship exists.
3. **Present** -> **Removed** when no inbound relationships remain after retention cleanup.
