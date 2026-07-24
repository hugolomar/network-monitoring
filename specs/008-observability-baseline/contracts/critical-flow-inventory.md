# Critical Flow Inventory

## Purpose

Define the authoritative critical-flow list used to validate SC-003 coverage and alerting obligations.

## Version

- Inventory version: `v1`
- Last updated: `2026-07-08`

## Flows

| Flow ID | Description | Services In Scope | Success Signal | Failure Signal |
|---------|-------------|-------------------|----------------|----------------|
| `graph-retention` | Scheduled retention sweep and stale-edge cleanup execution | `NetworkMonitoring.Backend` | `graph_retention_removed_total` emits expected maintenance counts without exceptions | Hosted-service execution logs/alerts indicate sweep failure or abnormal cleanup spikes |
| `device-intake` | Device ingestion from integration console to backend inventory | `NetworkMonitoring.IntegrationConsole`, `NetworkMonitoring.Backend` | Intake operations produce created/updated/idempotent outcomes | Intake persistence failure/rejected outcome with error telemetry |
| `graph-query` | Operator graph diagnostics retrieval path | `NetworkMonitoring.Frontend`, `NetworkMonitoring.Backend` | Graph query endpoints return successful payloads with trace/correlation context | `GRAPH_UNAVAILABLE` or invalid-request errors with correlated diagnostics |

## Governance

- Any new production critical flow MUST be added to this inventory in the same change set.
- SC-003 validation MUST reference this file as the source of truth for "100% of defined critical flows".
