# Contract: Probe Traffic Capture Port

## Purpose
Define the application-layer input contract used by probe use cases to receive traffic-derived
observations without coupling use-case logic to tshark.

## Port Naming
- Canonical interface for this feature: `ITrafficProvider`.

## Port Interface (Conceptual)
- `ReadObservations(cancellationToken)` returning a stream/sequence of normalized traffic records.

## Behavioral Contract
- The provider yields observation records in detection order.
- The provider does not apply domain-specific business rules; it only normalizes capture data.
- Malformed capture lines are reported as diagnostics and skipped without terminating the stream.
- End-of-stream and cancellation are handled gracefully by the provider.
- Observation-level business validation is performed in Application using explicit validation
  results (error accumulation + skip invalid), not exception-driven normal flow.

## Input Mode Selection Contract
- Host configuration selects exactly one `ITrafficProvider` adapter per probe run.
- `Live` mode maps to interface-based capture (`TsharkTrafficProvider`).
- `DeterministicTest` mode maps to deterministic capture-source ingestion (for example
  `PcapFileTrafficProvider`).
- Deterministic test mode requires explicit source configuration and MUST fail fast at startup when
  source configuration is invalid.
- Input mode selection MUST NOT alter downstream application behavior (validation, deduplication, or
  publication semantics).

## Current Adapters
- `TsharkTrafficProvider` executes tshark capture and converts raw output lines into normalized
  observation records for `Live` mode.
- Deterministic test adapter (`PcapFileTrafficProvider`) will implement `ITrafficProvider` for
  replayable probe validation runs without live interface dependency.
- When `DeterministicPlaybackSpeed` is greater than zero, the deterministic adapter MUST delay
  between mapped observations according to PCAP timestamp deltas divided by the configured speed.
  When the speed is zero, observations MAY be delivered as fast as the capture reader allows.

## Future Adapter Examples
- `OtherSensorTrafficProvider` for alternative capture technologies.

## Compatibility Rule
- Any adapter replacing tshark must implement `ITrafficProvider` and preserve the normalized
  observation contract expected by Application use cases.

## Implementation Note (Current State)
- Implemented adapter: `TsharkTrafficProvider` in
  `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkTrafficProvider.cs`.
- Implemented deterministic adapter: `PcapFileTrafficProvider` in
  `src/NetworkMonitoring.Probe/Infrastructure/Traffic/PcapFileTrafficProvider.cs`.
- Implemented mapper: `TsharkObservationMapper` in
  `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkObservationMapper.cs`.
- Shared domain entities/value objects consumed by the adapter pipeline are in
  `src/NetworkMonitoring.Domain/Shared/Entities/` and
  `src/NetworkMonitoring.Domain/Shared/ValueObjects/`.
