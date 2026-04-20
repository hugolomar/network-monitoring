# Data Model: Probe Session Detection Visibility

## Overview
This increment models session-domain artifacts emitted by the probe:
- `Session` (entity)

Session artifacts are part of the shared domain and inherit from SeedWork abstractions per
constitution constraints.

Application flow also uses an observation validation result object to capture input-validation
errors before domain emission.

## Session emission policy (console / event stream)

The same **logical** session detection outcome is emitted to **operator-visible output** (JSONL) and,
when enabled, to the **event stream** (Kafka value per `session-detected-value.avsc`). Field
semantics MUST stay aligned; only encoding differs (JSON text vs Avro bytes).

Before dispatching a `SessionDetected` record, the probe MAY apply **emission deduplication**:
- **Identity key**: normalized source IP, destination IP, source port, destination port, and
  protocol for the validated observation.
- **Window**: configurable duration (minutes); default 10 when not set; zero or negative disables
  deduplication.
- **Scope**: in-memory within a single probe process; restarting the probe clears history.

This reduces noise for long-lived or high-frequency flows without changing how `Session` entities are
built from each observation.

## Application Validation Model

- `ObservationValidationResult`:
  - `IsValid` (bool)
  - `Errors` (collection of validation messages)
- Role:
  - Aggregates validation issues from incoming observations.
  - Allows dropping invalid observations while keeping stream processing active.
  - Keeps exception handling focused on unexpected runtime failures.

## Entity: Session

### Purpose
Represents a detected network communication observation suitable for monitoring queries and
future event publication.

### Required Attributes
- `Id` (nullable integer, inherited from `Entity`): `null` until a persistent identifier is assigned
  by infrastructure (e.g. database) in a later increment; the probe does not allocate surrogate keys.
- `SourceIp` (`IpAddress` ValueObject, IPv4/IPv6)
- `DestinationIp` (`IpAddress` ValueObject, IPv4/IPv6)
- `SourcePort` (`Port` ValueObject, optional when protocol does not use ports)
- `DestinationPort` (`Port` ValueObject, optional when protocol does not use ports)
- `Protocol` (`ProtocolType` ValueObject, normalized, e.g., TCP/UDP/ICMP/OTHER)
- `FirstSeenUtc` (datetime)
- `LastSeenUtc` (datetime, must be >= `FirstSeenUtc`)
- `BytesObserved` (long, >= 0)

### Validation Rules
- IP fields must be parseable addresses.
- Ports must be in range 1..65535 when present.
- Protocol value must be normalized to known set or `OTHER`.
- `LastSeenUtc >= FirstSeenUtc`.
- `BytesObserved >= 0`.

## Required ValueObjects
- `IpAddress` inherits from `SeedWork/ValueObject`.
- `Port` inherits from `SeedWork/ValueObject`.
- `ProtocolType` inherits from `SeedWork/ValueObject`.

## Optional Next-Iteration ValueObjects
- `Hostname` (if stricter hostname normalization/validation is needed).
- `VlanId` (if VLAN rules and ranges become relevant).
- `NetworkInterfaceName` (if interface naming constraints must be enforced).

## State Notes
- Session entities are append/update over observation time windows.
- This phase is read-emit for validation only; persistence lifecycle is out of scope.
