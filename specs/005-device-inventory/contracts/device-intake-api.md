# Contract: Device Inventory Intake API

## Purpose

Define the real backend contract that replaces the fake/test receiver used by
`004-device-ingestion`.

## Endpoint

`POST /devices`

## Headers

- `Content-Type: application/json`
- `Idempotency-Key: <normalized-mac>`

`Idempotency-Key` must identify the same normalized MAC address as request body `macAddress`.

## Request Body

Compatible with `specs/004-device-ingestion/contracts/device-intake-http.md`:

```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "primaryIp": "192.168.1.10",
  "hostname": "switch-01",
  "observedIps": ["192.168.1.10", "192.168.1.11"],
  "firstSeenUtc": "2026-04-27T12:00:00.0000000+00:00",
  "lastSeenUtc": "2026-04-27T12:05:00.0000000+00:00",
  "discoverySource": "TRAFFIC",
  "sourceEvent": {
    "eventType": "DeviceDetected",
    "source": "probe",
    "schemaVersion": 1,
    "occurredAtUtc": "2026-04-27T12:05:01.0000000+00:00"
  }
}
```

## Successful Outcomes

- `201 Created`: a new device inventory record was created.
- `200 OK`: an existing device was updated or consolidated.
- `200 OK` or `204 No Content`: an exact duplicate/idempotent request was accepted without creating a
  duplicate record.

The final status-code split may be refined in implementation tasks, but all successful outcomes must
be treated as success by the Integration Console.

## Rejection Outcomes

- `400 Bad Request`: malformed request, missing required fields, missing/malformed idempotency key,
  invalid MAC/IP values, mismatched MAC identity, or invalid timestamp ordering.
- `415 Unsupported Media Type`: unsupported request content type.
- `500 Internal Server Error`: unexpected backend failure.
- `503 Service Unavailable`: persistence dependency unavailable.

## Contract Rules

- The backend must not create duplicate stored devices for repeated requests with the same normalized
  MAC identity.
- Validation failure must not partially update stored device state.
- The response should include enough diagnostic detail for local/operator validation without exposing
  sensitive internals.
