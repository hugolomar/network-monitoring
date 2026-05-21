# Contract: Backend Device Contracts Consumed by UI

## Purpose

Document how the Device Management UI consumes existing backend contracts from
`005-device-inventory`. This file does not define new backend behavior.

## Inventory Query

### Request

`GET /devices`

### Successful Response

`200 OK`

```json
{
  "items": [
    {
      "id": 1,
      "macAddress": "AA:BB:CC:DD:EE:FF",
      "primaryIp": "192.168.1.10",
      "hostname": "switch-01",
      "observedIps": ["192.168.1.10", "192.168.1.11"],
      "firstSeenUtc": "2026-04-27T12:00:00.0000000+00:00",
      "lastSeenUtc": "2026-04-27T12:05:00.0000000+00:00",
      "discoverySource": "TRAFFIC"
    }
  ]
}
```

### Empty Inventory Response

```json
{
  "items": []
}
```

### UI Behavior

- `200 OK` with one or more items maps to loaded inventory.
- `200 OK` with empty items maps to empty inventory.
- Unreachable backend or `503 Service Unavailable` maps to backend-unavailable state.
- Other unexpected failures map to a recoverable error state.

## Manual Device Creation

### Request

`POST /devices`

### Headers

- `Content-Type: application/json`
- `Idempotency-Key: <normalized-mac>`

### Body

```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "primaryIp": "192.168.1.10",
  "hostname": "switch-01",
  "observedIps": ["192.168.1.10"],
  "firstSeenUtc": "2026-04-27T12:00:00.0000000+00:00",
  "lastSeenUtc": "2026-04-27T12:05:00.0000000+00:00",
  "discoverySource": "MANUAL",
  "sourceEvent": null
}
```

### UI Behavior

- `201 Created` maps to created-success state.
- `200 OK` maps to idempotent/consolidated success state.
- `400 Bad Request` maps to validation-error state using backend-provided reason when available.
- `415 Unsupported Media Type` should not occur from the UI client and is treated as a client bug.
- `503 Service Unavailable` maps to backend-unavailable state.
- Unexpected failures map to a recoverable error state.

## Compatibility Rules

- The UI must not require backend schema fields beyond the existing `005-device-inventory` contracts.
- The UI must not require changes to `GET /devices` or `POST /devices`.
- The UI must not use Kafka topics, database tables, or shared .NET domain entities as contracts.
