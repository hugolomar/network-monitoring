# Quickstart: Device Ingestion

## Goal

Run the Integration Console against the reference Kafka stack and a fake HTTP receiver to validate that
`DeviceDetected` events from `devices.detected` are forwarded to `POST /devices`.

## Prerequisites

- .NET 10 SDK.
- Docker with the reference Kafka stack available.
- `003-device-discovery` completed so `DeviceDetected` events can be produced to `devices.detected`.

## Start Reference Kafka

```bash
docker compose -f docker-compose.reference-stack.yml up -d
./scripts/bootstrap/kafka-topics-init.sh
```

Verify the device topic:

```bash
docker compose -f docker-compose.reference-stack.yml exec -T kafka-1 kafka-topics \
  --bootstrap-server kafka-1:29092 \
  --describe \
  --topic devices.detected
```

## Start Fake Device Receiver

The implementation includes a test/fake receiver in
`tests/NetworkMonitoring.IntegrationConsole.IntegrationTests/Support/FakeDeviceReceiver.cs`. It records
received requests and exposes enough diagnostics for tests to confirm:

- request path is `/devices`,
- `Idempotency-Key` equals the normalized MAC,
- request body matches `contracts/device-intake-http.md`,
- duplicate requests with the same MAC produce no duplicate downstream effect.

## Run Integration Console

Expected configuration shape:

```bash
IntegrationConsole__KafkaBootstrapServers=localhost:9092,localhost:9093,localhost:9094 \
IntegrationConsole__SchemaRegistryUrl=http://localhost:8081 \
IntegrationConsole__KafkaDeviceTopic=devices.detected \
IntegrationConsole__KafkaConsumerGroupId=device-ingestion-local \
IntegrationConsole__BackendBaseUrl=http://localhost:5080 \
IntegrationConsole__RetryMaxAttempts=3 \
dotnet run --project src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj
```

## Produce Device Events

Run the probe with Kafka enabled, or run the gated integration producer/consumer tests from
`003-device-discovery` to create sample `DeviceDetected` messages.

```bash
Probe__EnableKafka=true \
Probe__EnableConsole=true \
Probe__KafkaBootstrapServers=localhost:9092,localhost:9093,localhost:9094 \
Probe__SchemaRegistryUrl=http://localhost:8081 \
Probe__KafkaDeviceTopic=devices.detected \
dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj
```

## Validate

Confirm the fake receiver recorded one `POST /devices` request per valid sampled event and that
malformed/invalid cases are logged rather than crashing the Integration Console.

Expected validation checks:

- valid `DeviceDetected` -> `POST /devices`,
- Kafka key and body MAC match,
- `Idempotency-Key` is normalized MAC,
- transient receiver failure retries,
- permanent validation failure is rejected,
- malformed events do not block later valid events.

Run the automated validation:

```bash
dotnet test src/NetworkMonitoring.sln
```

Run Kafka-gated validation only after the reference stack is running:

```bash
RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter "FullyQualifiedName~KafkaDeviceIngestionIntegrationTests"
```

## Build Container

```bash
docker build -f src/NetworkMonitoring.IntegrationConsole/Dockerfile \
  -t network-monitoring-integration-console:local .
```

Validation result recorded during implementation: the Dockerfile built successfully as
`network-monitoring-integration-console:local`.

## Stop

```bash
docker compose -f docker-compose.reference-stack.yml down
```
