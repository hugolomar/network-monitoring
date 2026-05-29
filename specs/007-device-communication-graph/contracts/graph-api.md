# Contract: Device Communication Graph API

## Purpose

Define the retrieval contract used by visualization consumers to fetch a bounded communication subgraph
centered on a root device identity.

## Endpoint

`GET /api/graph/devices`

## Query Parameters

- `rootDeviceId` (required): root internal device identity for traversal.
- `depth` (optional): requested traversal depth; effective value is capped by configuration.
- `limit` (optional): requested maximum number of nodes in response; effective value is bounded by configuration.
- If `depth` is omitted, default value is `1`; maximum effective value is `3`.
- If `limit` is omitted, default value is `200`; maximum effective value is `500`.

## Access Policy

- The endpoint requires authenticated access.
- The endpoint enforces role-based authorization.
- Allowed graph-read roles in this feature scope: `admin`, `analyst`, `auditor`, `integration`.

## Successful Response

`200 OK`

```json
{
  "nodes": [
    {
      "id": "device-1",
      "kind": "InternalDevice"
    },
    {
      "id": "203.0.113.20",
      "kind": "ExternalHost"
    }
  ],
  "edges": [
    {
      "sourceId": "device-1",
      "destinationId": "203.0.113.20",
      "protocol": "TCP",
      "weight": 4,
      "firstSeenUtc": "2026-05-20T10:00:00Z",
      "lastSeenUtc": "2026-05-21T10:00:00Z"
    }
  ],
  "truncated": false
}
```

## Response Rules

- `nodes` includes root and reachable neighbors subject to effective depth and limit.
- `edges` contains communication relationships between returned nodes.
- `truncated = true` when result-size limits are reached before traversal completion.

## Error Responses

### Invalid request

`400 Bad Request` when required parameters are missing or invalid.

```json
{
  "code": "GRAPH_INVALID_REQUEST",
  "message": "The graph query parameters are invalid.",
  "traceId": "00-abc123...",
  "timestampUtc": "2026-05-21T18:00:00Z"
}
```

### Forbidden

`403 Forbidden` when caller is authenticated but does not have an allowed graph-read role.

### Graph store unavailable

`503 Service Unavailable`

```json
{
  "code": "GRAPH_UNAVAILABLE",
  "message": "Communication graph is temporarily unavailable.",
  "traceId": "00-abc123...",
  "timestampUtc": "2026-05-21T18:00:00Z"
}
```

## Compatibility Rules

- The contract is additive; existing non-graph endpoints remain unaffected.
- Additional response fields may be introduced in backward-compatible form.
- Existing fields and their semantic meaning must remain stable across compatible versions.
- Graph-store outage behavior is isolated to this endpoint family and must not alter non-graph endpoint contracts.
