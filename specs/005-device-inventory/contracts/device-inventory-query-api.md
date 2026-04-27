# Contract: Device Inventory Query API

## Purpose

Define the minimal read contract needed to validate stored devices and support the later
`006-device-ui` feature.

## Endpoint

`GET /devices`

## Response: 200 OK

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

## Empty Inventory

When no devices exist, the backend returns:

```json
{
  "items": []
}
```

## Contract Rules

- Each normalized MAC appears at most once in `items`.
- The response reflects consolidated inventory state, not raw intake attempts.
- The endpoint is read-only and must not mutate inventory state.
- Authentication/RBAC is deferred for this increment, but the contract is intended to be protected
  before user-facing UI exposure.
