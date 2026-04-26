# Quickstart: Device Discovery Separation

## Goal
Validate device discovery using structured `DeviceDetected` output.

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

## Steps
1. Build and run probe in local mode.
2. Feed representative observation traffic.
3. Verify `DeviceDetected` records are emitted with required schema fields.
4. Feed invalid discovery evidence and verify diagnostics are logged while processing continues.
5. Feed repeated detections for same identity and verify lifecycle/consolidation semantics.

## Validation Commands (example flow)
- `dotnet test src/NetworkMonitoring.Probe.sln`
- `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- `timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- `docker compose -f docker-compose.probe.yml up --build`

## Expected Outcomes
- Valid device detections match contract schema.
- Invalid discovery inputs do not crash processing.
- Repeated detections follow deterministic lifecycle behavior.

## Validation Notes (2026-04-19)
- Automated validation command executed: `dotnet test src/NetworkMonitoring.Probe.sln`.
- Observed result: all non-Kafka-gated probe tests passed; current suite reports 24 unit tests passed,
  2 integration tests passed, and 1 Kafka integration test skipped unless `RUN_KAFKA_INTEGRATION=1`.
- Coverage highlights:
  - invalid discovery rejection with continuation,
  - schema-level `DeviceDetected` field validation,
  - repeated-detection consolidation scenario for device lifecycle timestamps.

## Out of Scope
- Backend persistence/inventory storage
- UI/API management flows
- Requirements not covered by `specs/003-device-discovery/`
