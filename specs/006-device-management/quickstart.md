# Quickstart: Device Management UI

## Goal

Run the Device Management UI as a separate deployable, view devices stored by the backend, and create a
manual device through the existing backend API.

## Prerequisites

- Node.js LTS for local UI development.
- Docker for container/reference stack validation.
- Device Inventory backend from `005-device-inventory`.

## Start Backend Dependencies

From the repository root:

```bash
docker compose -f docker-compose.reference-stack.yml up -d postgres network-monitoring-backend
```

Validate the backend:

```bash
curl -i http://localhost:5090/devices
```

Expected empty response when no devices exist:

```json
{"items":[]}
```

## Seed a Device for UI Validation

```bash
curl -i -X POST http://localhost:5090/devices \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: AA:BB:CC:DD:EE:FF" \
  -d '{
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "primaryIp": "192.168.1.10",
    "hostname": "switch-01",
    "observedIps": ["192.168.1.10"],
    "firstSeenUtc": "2026-04-27T12:00:00.0000000+00:00",
    "lastSeenUtc": "2026-04-27T12:05:00.0000000+00:00",
    "discoverySource": "MANUAL",
    "sourceEvent": null
  }'
```

## Run UI Locally

Planned local development flow:

```bash
cd src/NetworkMonitoring.Frontend
npm install
npm run dev
```

Configure the UI to use:

```text
http://localhost:5090
```

Open the documented local UI URL from the dev server and verify:

- The seeded device appears.
- Refresh reloads the inventory without a full browser restart.
- Backend unavailable state appears if the backend is stopped.
- Manual creation can add or consolidate a device.

## Run UI in Reference Stack

Planned container validation flow:

```bash
docker compose -f docker-compose.reference-stack.yml up -d --build network-monitoring-backend network-monitoring-device-management-ui
```

Open the documented UI port and verify it can reach the backend service in the Compose network.

## Test Plan

```bash
cd src/NetworkMonitoring.Frontend
npm test
npm run build
```

Expected coverage:

- Inventory loading with devices.
- Empty inventory.
- Backend unavailable.
- Refresh failure with previous inventory retained.
- Manual creation success.
- Backend validation rejection.
- Idempotent/consolidated success.

## Out of Scope

- Keycloak/RBAC login.
- mTLS.
- Session search or dashboards.
- Backend contract changes.
- Kafka, PostgreSQL, Elasticsearch, Probe, or Integration Console internals.
