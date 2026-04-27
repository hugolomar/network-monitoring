# Quickstart: Device Inventory

## Goal

Run the real Device Inventory backend locally, accept `POST /devices` requests, persist device state,
and query the stored inventory with `GET /devices`.

## Prerequisites

- .NET 10 SDK.
- Docker for PostgreSQL and container validation.
- `004-device-ingestion` completed if validating Integration Console forwarding.

## Start Local Dependencies

Start the reference stack when validating PostgreSQL-backed persistence or the full ingestion handoff:

```bash
docker compose -f docker-compose.reference-stack.yml up -d postgres network-monitoring-backend
```

For the full probe/Kafka/Integration Console path, also start the Kafka services from the same
reference stack and initialize topics as documented in earlier feature quickstarts.

## Initialize Database Schema

The backend schema is managed with EF Core migrations committed under
`src/NetworkMonitoring.Backend/Infrastructure/Persistence/Migrations/`.

Local validation applies pending migrations on backend startup when
`Backend__ApplyMigrationsOnStartup=true`. The validation result must prove that accepted devices survive
a backend process restart while using the PostgreSQL container.

## Run Backend Directly

When running the backend directly against the PostgreSQL container:

```bash
Backend__ConnectionString="Host=localhost;Port=5432;Database=network_monitoring;Username=network_monitoring;Password=network_monitoring" \
dotnet run --project src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj
```

## Validate Direct Intake

Submit a valid device intake request:

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
    "discoverySource": "TRAFFIC",
    "sourceEvent": {
      "eventType": "DeviceDetected",
      "source": "probe",
      "schemaVersion": 1,
      "occurredAtUtc": "2026-04-27T12:05:01.0000000+00:00"
    }
  }'
```

Query the inventory:

```bash
curl -i http://localhost:5090/devices
```

Expected behavior:

- One device appears for `AA:BB:CC:DD:EE:FF`.
- Repeating the same `POST /devices` request does not create a duplicate device.
- Invalid `Idempotency-Key` or MAC mismatch is rejected.

## Validate With Integration Console

After the backend is running, point the Integration Console at it:

```bash
IntegrationConsole__BackendBaseUrl=http://localhost:5090 \
dotnet run --project src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj
```

With Kafka and the probe producing `DeviceDetected`, the Integration Console should forward valid
devices to the real backend, and `GET /devices` should show persisted inventory state.

When both services run inside Docker Compose, configure the Integration Console backend target as
`http://network-monitoring-backend:8080`.

## Automated Validation

```bash
dotnet test src/NetworkMonitoring.sln
```

## Container Validation

Expected image build:

```bash
docker build -f src/NetworkMonitoring.Backend/Dockerfile \
  -t network-monitoring-backend:local .
```

Expected compose service build:

```bash
docker compose -f docker-compose.reference-stack.yml build network-monitoring-backend
```

## Stop

Stop the backend process and local Compose services used for PostgreSQL/Kafka validation.
