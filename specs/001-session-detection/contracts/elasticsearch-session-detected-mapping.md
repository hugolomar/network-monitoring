# Avro `SessionDetected` → Elasticsearch document fields

**Source contract**: `session-detected-value.avsc` (`net.networkmonitoring.events.SessionDetected`).  
**US3 purpose**: `FR-006` (normalized comparable values), `FR-018` (same semantics as stream/console, no second definition of “session”).

## Field mapping (1:1 names)

| Avro field         | ES mapping intent | Notes |
|--------------------|------------------|--------|
| `eventType`        | `keyword`        | e.g. `SessionDetected` |
| `occurredAtUtc`    | `date` (optional format `strict_date_optional_time`) | ISO-8601 instant string from probe |
| `source`           | `keyword`        | Producer id |
| `schemaVersion`    | `integer`        | Contract version |
| `sessionId`        | `integer`        | May be `null` in Avro; omit or `null` in JSON |
| `sourceIp`         | `keyword`        | Normalized IP string (FR-006) |
| `destinationIp`     | `keyword`        | Same |
| `sourcePort`        | `integer`        | Nullable |
| `destinationPort`   | `integer`        | Nullable |
| `protocol`         | `keyword`        | Normalized protocol name |
| `firstSeenUtc`     | `date`           | ISO-8601 window start |
| `lastSeenUtc`      | `date`           | ISO-8601 window end |
| `bytesObserved`    | `long`          | Non-negative byte count |

## Index identity & duplicates

- **Kafka key**: String partition key (same identity as stream duplicate suppression) — the Elasticsearch Sink may use it as **document `_id`** when `key.ignore` is `false` (see `scripts/connectors/elasticsearch-sink-sessions-detected.json`), aligning **query-side** idempotency with stream semantics where the connector allows.
- **Overlap in query results** (replays, reindexes): still governed by the spec **Edge Cases (US3)** and **FR-011** / **FR-013** / **FR-018**: projection docs must not contradict the **declared contract**; any deduplication for analysts is **documented** in `quickstart.md` and connector notes.

## Index naming

- Reference index (via connector `topic.to.external.resource.mapping`): `sessions-detected` (see `scripts/connectors/elasticsearch-sink-sessions-detected.json`).  
- Template: `scripts/bootstrap/elasticsearch/index-template-sessions-detected.json` applies to `sessions-detected*`.
