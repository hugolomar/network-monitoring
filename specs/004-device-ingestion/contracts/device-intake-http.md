# Contract: Device Intake HTTP

## Purpose

Define the outbound HTTP request emitted by the Integration Console after consuming a valid
`DeviceDetected` event. The real backend implementation belongs to `005-device-backend`; this contract
is validated in 004 with a fake/test receiver.

## Endpoint

`POST /devices`

The backend base URL is configurable. The Integration Console appends `/devices` to that base URL for
device intake.

## Headers

- `Content-Type: application/json`
- `Idempotency-Key: <normalized-mac>`

`Idempotency-Key` MUST equal the normalized MAC address in the request body.

## Request Body

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

## Field Semantics

- `macAddress`: Required normalized MAC identity from the Kafka key and payload.
- `primaryIp`: Optional primary observed IP.
- `hostname`: Optional hostname.
- `observedIps`: Optional observed IP evidence; empty list is allowed.
- `firstSeenUtc` / `lastSeenUtc`: Required detection timestamps.
- `discoverySource`: Required discovery source from the event.
- `sourceEvent`: Minimal event metadata for diagnostics and traceability.

## Expected Receiver Outcomes

- `2xx`: request accepted; ingestion attempt succeeds.
- `408`, `429`, `5xx`, timeout, or network failure: transient failure; retry according to configured
  policy.
- `400` or `422`: permanent validation rejection; do not retry indefinitely.
- `409`: reserved for backend-defined duplicate/idempotency behavior in 005. Until the backend contract
  finalizes this response, tests should validate idempotency using repeated requests with the same
  `Idempotency-Key` against a fake receiver.

## Out of Scope

- Backend persistence behavior.
- `GET /devices`.
- Authentication and authorization policy beyond configurable service transport/security settings.
