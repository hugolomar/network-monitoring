# Contract: Device Management UI States

## Purpose

Define observable UI states that implementation and tests must cover. These are user-facing behavior
contracts, not backend or event contracts.

## Inventory Loading States

| State | Trigger | Required UI Behavior |
|-------|---------|----------------------|
| Initial | Page first opens | Begin inventory load or present loading affordance |
| Loading | Inventory request in progress | Show loading state and avoid stale success messaging |
| Loaded | `GET /devices` returns one or more items | Display inventory rows with required fields |
| Empty | `GET /devices` returns `items: []` | Display explicit empty inventory message |
| Backend Unavailable | Backend unreachable or unavailable | Display recoverable error and retry action |

## Refresh States

| State | Trigger | Required UI Behavior |
|-------|---------|----------------------|
| Refreshing | Operator refreshes after inventory loaded | Keep previous inventory visible while refresh runs |
| Refresh Failed | Refresh request fails | Keep previous inventory visible and show latest-refresh failure |
| Refreshed | Refresh succeeds | Replace visible inventory with latest backend response |

## Manual Creation States

| State | Trigger | Required UI Behavior |
|-------|---------|----------------------|
| Editing | Operator enters manual device details | Preserve form state until submit/cancel |
| Submitting | Operator submits valid-looking draft | Disable duplicate submit affordance while request is in flight |
| Created Success | Backend returns created outcome | Show success and refresh/update inventory |
| Idempotent Success | Backend returns existing/consolidated/idempotent success | Show successful consolidation, not duplicate error |
| Validation Error | Backend rejects request or lightweight form validation fails | Show operator-readable reason and keep form editable |
| Backend Unavailable | Backend unavailable during submission | Show recoverable failure and allow retry |

## Required Test Coverage

- Inventory loading with non-empty response.
- Empty inventory response.
- Backend unavailable during initial load.
- Refresh failure after existing inventory.
- Manual creation success.
- Backend validation rejection.
- Idempotent/consolidated success for an existing MAC.
