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

## Current Adapter (This Phase)
- `TsharkTrafficProvider` executes tshark capture and converts raw output lines into normalized
  observation records.

## Future Adapter Examples
- `PcapFileTrafficProvider` for replay/testing.
- `OtherSensorTrafficProvider` for alternative capture technologies.

## Compatibility Rule
- Any adapter replacing tshark must implement `ITrafficProvider` and preserve the normalized
  observation contract expected by Application use cases.

## Implementation Note (Current State)
- Implemented adapter: `TsharkTrafficProvider` in
  `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkTrafficProvider.cs`.
- Implemented mapper: `TsharkObservationMapper` in
  `src/NetworkMonitoring.Probe/Infrastructure/Traffic/TsharkObservationMapper.cs`.
- Shared domain entities/value objects consumed by the adapter pipeline are in
  `src/NetworkMonitoring.Domain/Shared/Entities/` and
  `src/NetworkMonitoring.Domain/Shared/ValueObjects/`.
