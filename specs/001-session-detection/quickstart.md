# Quickstart: Probe Session Detection Visibility

## Goal
Run the probe in local validation mode and verify that session entities are emitted to
console as structured records.

## Prerequisites
- .NET 10 SDK installed
- `tshark` installed and available in PATH
- Network interface or capture source available for test traffic
- Docker engine with compose plugin (for containerized execution path)

## Configuration (session scope)
- Application settings section `Probe` (see `src/NetworkMonitoring.Probe/appsettings.json`):
  - `TSharkPath`: capture executable (default `tshark`).
  - `InterfaceName`: capture interface (default `eth0`; adjust to your environment).
  - `CaptureFilter`: optional BPF-style filter string.
  - `SessionDeduplicationWindowMinutes`: sliding window for suppressing duplicate `SessionDetected`
    emissions for the same session identity (default `10`; use `0` to disable).

## Steps
1. Restore and build solution/projects.
2. Start the probe module in console output mode.
3. Generate or observe representative network traffic.
4. Confirm console emits `SessionDetected` records.
5. Introduce malformed/partial sample input and verify processing continues.

## Expected Outcomes
- At least one valid session record is printed.
- Repeated similar traffic produces stable payload structures.
- Malformed observations are dropped with diagnostics, without stopping the probe.
- Invalid observations report explicit validation errors in logs and do not rely on exception
  control flow for normal handling.

## Validation Commands (example flow)
- Build and test:
  - `dotnet test src/NetworkMonitoring.Probe.sln`
- Start probe:
  - `dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Optional startup smoke check:
  - `timeout 8 dotnet run --project src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj`
- Build container image:
  - `docker build -f src/NetworkMonitoring.Probe/Dockerfile -t network-monitoring-probe:local .`
- Run probe container (host networking + capture capabilities):
  - `docker compose -f docker-compose.probe.yml up --build`

## Optional: Event stream (Kafka) validation (US2 / SC-005)

When implementing or validating stream publication:

1. Start local **Kafka (KRaft) + Schema Registry** using the compose or runbook added for this
   feature (see plan / tasks).
2. Ensure topic **`sessions.detected`** (or configured override) exists with appropriate partitions;
   register or verify Avro subject **`sessions.detected-value`** per `contracts/session-detected-avro.md`.
3. Configure probe settings for bootstrap servers, registry URL, topic, SSL/mTLS (or documented dev
   relaxation).
4. Run probe with publication enabled; consume messages and confirm payloads match the Avro contract.

## Out of scope (other modules)

- Session persistence in a backend database
- UI/API exposure for sessions
- Device discovery publication (separate feature `002-device-discovery`)
