# Contract: Device Communication Graph API

## Purpose

Define retrieval contracts used by visualization consumers to fetch either a bounded communication
subgraph centered on a root identity or a bounded full graph snapshot.

## Endpoints

- `GET /api/graph/devices` (root-centered neighborhood)
- `GET /api/graph/devices/all` (full graph snapshot)

## Query Parameters

### `GET /api/graph/devices`

- `rootDeviceId` (required): root internal device identity for traversal.
- `depth` (optional): requested traversal depth; effective value is capped by configuration.
- `limit` (optional): requested maximum number of nodes in response; effective value is bounded by configuration.
- If `depth` is omitted, default value is `1`; maximum effective value is `3`.
- If `limit` is omitted, default value is `200`; maximum effective value is `500`.

### `GET /api/graph/devices/all`

- `limit` (optional): requested maximum number of nodes in response; effective value is bounded by configuration.
- If `limit` is omitted, default value is `200`; maximum effective value is `500`.

## Access Policy

- Endpoints require authenticated access.
- Endpoints enforce role-based authorization.
- Allowed graph-read roles in this feature scope: `admin`, `analyst`, `auditor`, `integration`.
- Runtime header mapping in the current backend implementation:
  - `Authorization` header is required for authenticated access checks.
  - `X-Role` header provides the effective caller role for authorization checks.

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

- For `/api/graph/devices`, `nodes` includes root and reachable neighbors subject to effective depth and limit.
- For both endpoints, `edges` contains communication relationships between returned nodes.
- `truncated = true` when result-size limits are reached before traversal completion.
- For `/api/graph/devices/all`, `nodes` contains the bounded snapshot subset and `edges` includes only
  relationships where both endpoints are in the returned node set.

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

### Unauthorized

`401 Unauthorized` when the caller does not provide authentication.

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

## UI Consumption Rules

- Graph UI may call `/api/graph/devices/all` when root identity is not provided.
- Graph UI may call `/api/graph/devices` when root identity is provided.
- UI must treat `truncated=true` as a partial-view warning for either endpoint.
- UI inventory-correlation hints are best-effort client-side matching and do not imply canonical identity
  authority transfer from inventory to graph.

## Compatibility Rules

- The contract is additive; existing non-graph endpoints remain unaffected.
- Additional response fields may be introduced in backward-compatible form.
- Existing fields and their semantic meaning must remain stable across compatible versions.
- Graph-store outage behavior is isolated to this endpoint family and must not alter non-graph endpoint contracts.
