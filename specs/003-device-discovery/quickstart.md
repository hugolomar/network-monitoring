# Quickstart: Device Discovery Separation

## Goal
Validate device discovery using structured `DeviceDetected` output and optional publication to Kafka
topic `devices.detected`.

## Prerequisites
- .NET 10 SDK installed
- `tshark` available in PATH
- Test traffic source with observable MAC/IP evidence
- Docker engine (optional containerized path)

## Probe configuration (device discovery)
Under `Probe` in `src/NetworkMonitoring.Probe/appsettings.json`, settings relevant here include
`DeviceDeduplicationWindowMinutes` (default `10`; use `0` to emit every consolidated update). Capture
runtime settings such as `TSharkPath`, `InterfaceName`, and `CaptureFilter` are required for live
traffic but are shared with the probe host; normative behavior for those keys is outside this
quickstart unless repeated in `spec.md`. Domain consolidation in `Device` still applies on every
valid observation regardless of emission throttling.

For event-stream publication, configure:
- `EnableKafka`: `true` to publish detected devices to Kafka.
- `EnableConsole`: `true` / `false` for operator-visible JSONL output.
- `KafkaBootstrapServers`, `SchemaRegistryUrl`, and `KafkaDeviceTopic` (`devices.detected` default).
- Optional TLS/mTLS settings: `KafkaSecurityProtocol`, `KafkaSslCaLocation`,
  `KafkaSslCertificateLocation`, and `KafkaSslKeyLocation`.

## Steps
1. Build and run probe in local mode.
2. Feed representative observation traffic.
3. Verify `DeviceDetected` records are emitted with required schema fields.
4. Feed invalid discovery evidence and verify diagnostics are logged while processing continues.
5. Feed repeated detections for same identity and verify lifecycle/consolidation semantics.
6. When Kafka publication is enabled, consume `devices.detected` and verify Avro payload fields and
   normalized-MAC message keys.

## Validation Commands (example flow)
- `dotnet test src/NetworkMonitoring.sln`
- `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- `timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- `docker compose -f docker-compose.probe.yml up --build`

## Event stream (Kafka + Schema Registry)

The reference `docker-compose.reference-stack.yml` uses PLAINTEXT host listeners for local validation
only. Integration, staging, and production should use TLS/mTLS for Kafka and Registry clients.

### Order of operations
1. From repo root, start the Kafka services:
   - `docker compose -f docker-compose.reference-stack.yml up -d kafka-1 kafka-2 kafka-3 schema-registry`
2. Wait until brokers report healthy, then create required topics:
   - `./scripts/bootstrap/kafka-topics-init.sh`
   Defaults: 3 partitions, replication factor 3. The device topic default is `devices.detected`.
3. Confirm the device topic exists:
   - `docker compose -f docker-compose.reference-stack.yml exec -T kafka-1 kafka-topics --bootstrap-server kafka-1:29092 --describe --topic devices.detected`
4. Schema Registry subject `devices.detected-value` is registered when the probe or tests first
   produce Avro values with auto-registration enabled. Governed environments should register
   `contracts/device-detected-value.avsc` through approved tooling.

### Validate publication
- Automated (opt-in):
  - `RUN_KAFKA_INTEGRATION=1 dotnet test src/NetworkMonitoring.sln --filter FullyQualifiedName~KafkaDeviceEventPublishIntegrationTests`
- Manual: consume `devices.detected`, deserialize Avro using Schema Registry, verify fields match
  `device-detected-value.avsc`, and verify each Kafka key equals the payload `macAddress`.

## Expected Outcomes
- Valid device detections match contract schema.
- Invalid discovery inputs do not crash processing.
- Repeated detections follow deterministic lifecycle behavior.
- With Kafka enabled, valid device detections are available on `devices.detected`.

## Validation Notes (2026-04-19)
- Automated validation command executed: `dotnet test src/NetworkMonitoring.sln`.
- Observed result: all non-Kafka-gated probe tests passed; current suite reports 24 unit tests passed,
  2 integration tests passed, and 1 Kafka integration test skipped unless `RUN_KAFKA_INTEGRATION=1`.
- Coverage highlights:
  - invalid discovery rejection with continuation,
  - schema-level `DeviceDetected` field validation,
  - repeated-detection consolidation scenario for device lifecycle timestamps.

## Out of Scope
- Downstream consumers of `devices.detected`
- Requirements not covered by `specs/003-device-discovery/`
