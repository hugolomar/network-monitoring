# Data Model: Session Indexing

## Overview

This increment models the query-side projection of session detections:

- `Session Index Document` (Elasticsearch document mapped from `SessionDetected`)

The source event contract remains `specs/001-session-detection/contracts/session-detected-value.avsc`.
The indexed document reflects that contract for search and filtering; it is not a shared-domain entity.

## Projection: Session Index Document

### Purpose

Supports operational and investigative queries over past session detections that have already been
published to Kafka.

### Required Fields

- `eventType`
- `occurredAtUtc`
- `source`
- `schemaVersion`
- `sourceIp`
- `destinationIp`
- `protocol`
- `firstSeenUtc`
- `lastSeenUtc`
- `bytesObserved`

### Optional Fields

- `sessionId`
- `sourcePort`
- `destinationPort`

### Query Dimensions

- Time range over event or session timestamps.
- Normalized source and destination addresses.
- Source and destination ports when applicable.
- Normalized protocol.

### State Notes

- Kafka is the durable event log.
- Elasticsearch is a query-optimized projection and may lag behind stream publication.
- Duplicate or overlapping documents caused by replay/reindexing must not change session semantics;
  operator-facing deduplication expectations are documented in `quickstart.md`.
