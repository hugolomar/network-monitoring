# Quickstart: Device Communication Graph

## Goal

Validate end-to-end communication graph behavior:

1. Enriched session events project into communication relationships.
2. Graph retrieval returns bounded node/edge neighborhoods.
3. Retention removes stale relationships and orphan external hosts.
4. Graph-store outage is isolated from existing non-graph endpoints.

## Prerequisites

- .NET 10 SDK.
- Local graph-store runtime available.
- Existing backend and session enrichment flow from previous features.

## Start required services

1. Start Neo4j + backend dependencies:

   ```bash
   docker compose -f docker-compose.reference-stack.yml up -d postgres neo4j network-monitoring-backend
   ```

2. Validate Neo4j Browser is reachable at `http://localhost:7474` and Bolt at `localhost:7687`.
3. Ensure backend graph configuration points to Neo4j (`Provider=Neo4j`).
4. Ensure enriched session facts are available to the projection path.

### Neo4j credentials (local reference stack)

- Username: `neo4j`
- Password: `networkmonitoring123`
- Browser URL: `http://localhost:7474`
- Bolt URI: `bolt://localhost:7687`

## Validate projection behavior

1. Produce enriched session events representing:
   - internal-to-internal communication,
   - internal-to-external communication,
   - duplicate/replay delivery of same event identity.
2. Confirm communication relationships appear for each logical identity.
3. Confirm replay does not duplicate relationship structure and only advances counters/timestamps.
4. Simulate transient projection-write failures and confirm bounded retries; on retry exhaustion, confirm
   diagnostics are emitted and later events continue processing.

## Validate graph retrieval contract

1. Call `GET /api/graph/devices` with a valid root and `depth=1`.
2. Confirm response contains `nodes`, `edges`, and `truncated`.
3. Call with depth above configured cap and confirm effective traversal is capped.
4. Call with low limit and confirm `truncated=true` when cap is reached.
5. Call without `depth`/`limit` and confirm defaults (`depth=1`, `limit=200`) are applied.
6. Call with `depth=0` and confirm `400 Bad Request` + `GRAPH_INVALID_REQUEST`.

Reference curl:

```bash
curl -sS -H "Authorization: Bearer test" -H "X-Role: analyst" \
  "http://localhost:5090/api/graph/devices?rootDeviceId=api-root&depth=1&limit=50"
```

## Validate retention behavior

1. Seed relationships with `lastSeenUtc` older than 90 days.
2. Trigger or wait for retention run cadence.
3. Confirm stale relationships are removed.
4. Confirm external-host nodes without inbound relationships are removed.
5. Confirm internal device identities are not removed by retention.

## Validate outage isolation

1. Simulate graph-store unavailability.
2. Confirm graph retrieval returns `503 Service Unavailable` payload with `code`, `message`, `traceId`,
   and `timestampUtc`.
3. Confirm non-graph device/session endpoints continue responding normally.

## Validate access policy

1. Call `GET /api/graph/devices` without authentication and confirm access is rejected by auth policy.
2. Call with authenticated but unauthorized role and confirm `403 Forbidden`.
3. Call with one allowed role (`admin`, `analyst`, `auditor`, `integration`) and confirm access succeeds.

## Validate observability baseline

1. Execute projection and retention scenarios from this quickstart.
2. Confirm structured logs exist for projection attempts, retries, failures, and retention runs.
3. Confirm metrics include counters and latency signals for projection and retention execution paths.

## Recorded validation evidence (2026-05-21)

- Graph-focused automated checks: `dotnet test tests/NetworkMonitoring.Backend.IntegrationTests/NetworkMonitoring.Backend.IntegrationTests.csproj /p:BuildProjectReferences=false --filter "Graph"` -> `Passed: 14, Failed: 0`.
- Local development note: full-solution graph test command with reference builds is currently blocked by a pre-existing compilation issue in `src/NetworkMonitoring.IntegrationConsole/Program.cs`; backend graph slice itself builds and graph tests pass under the scoped command above.

## Recorded validation evidence (2026-06-15)

- Backend runtime provider switched to Neo4j in non-testing environments.
- Reference stack includes Neo4j service (`neo4j:5.22`) with Browser/Bolt ports published.
- Smoke validation:
  - Seeded `api-root -> api-peer` relationship in Neo4j.
  - `GET /api/graph/devices?rootDeviceId=api-root&depth=1&limit=50` returned nodes/edges from Neo4j.

## Suggested automated checks

- Integration test: projection idempotency by identity.
- Integration test: retrieval depth/limit clamp and truncation behavior.
- Integration test: retention prune + orphan cleanup.
- Integration test: graph outage does not cascade to existing endpoint families.
- Integration test: projection retry exhaustion emits diagnostics and processing continuity.
- Integration test: authenticated access requirement on graph endpoint.
