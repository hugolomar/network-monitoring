# Data Model: Device Discovery Separation

## Overview
This increment focuses on discovery-domain artifacts only:
- `Device` (shared-domain entity and aggregate root)
- `DeviceDetected` record (contract payload)
- `DiscoveryValidationResult` (application validation result)

## Entity: Device

### Purpose
Represents an observed network asset discovered from passive observation evidence.

### Required Attributes
- `Id` (nullable integer, inherited from `Entity`): `null` until a persistent identifier is assigned
  by infrastructure (e.g. database) in a later increment; the probe does not allocate surrogate keys.
- `MacAddress` (`MacAddress` ValueObject)
- `FirstSeenUtc` (datetime)
- `LastSeenUtc` (datetime, must be >= `FirstSeenUtc`)
- `DiscoverySource` (`DiscoverySource` ValueObject)

### Optional/Conditional Attributes
- `PrimaryIp` (`IpAddress` ValueObject, optional when unknown)
- `Hostname` (string, optional)
- `ObservedIps` (set of `IpAddress` ValueObjects)

### Validation Rules
- MAC must be parseable and normalized.
- `PrimaryIp`, if present, must be valid.
- `ObservedIps` entries must be valid and unique.
- `LastSeenUtc >= FirstSeenUtc`.

### Consolidation Semantics
- First detection initializes `FirstSeenUtc` and `LastSeenUtc`.
- Later detections for same identity update `LastSeenUtc` with the max observed timestamp.
- Out-of-order detections update `FirstSeenUtc` with the min observed timestamp.
- Evidence enrichment adds unique observed IPs and accepts latest non-empty hostname.
- `PrimaryIp` is assigned on first valid detection and preserved for deterministic behavior.

### Probe orchestration note
- The probe correlates repeated observations by **normalized MAC** and applies consolidation via
  `Device.ConsolidateDetection` on every valid observation. A separate **emission deduplication**
  window (`DeviceDeduplicationWindowMinutes`) may suppress extra `DeviceDetected` lines when another
  emission for the same MAC occurred within the configured interval (minimum spacing per MAC; entries
  older than the interval are forgotten so memory does not grow unbounded); the aggregate is still
  updated in memory so the next allowed emission reflects the latest consolidated state. Registry
  state is **in-memory** for one process lifetime (see `ProcessObservationsUseCase`).

## Contract Entity: DeviceDetected Record

### Purpose
Represents one emitted, validated discovery output for downstream consumers.

### Required Fields
- `eventType`
- `occurredAtUtc`
- `source`
- `schemaVersion`
- `deviceId`
- `macAddress`
- `firstSeenUtc`
- `lastSeenUtc`
- `discoverySource`

### Optional Fields
- `primaryIp`
- `hostname`
- `observedIps`

### Event Stream Semantics
- Default Kafka topic: `devices.detected`.
- Default Schema Registry subject: `devices.detected-value`.
- Kafka message key: normalized `macAddress`.
- Kafka value: Avro record defined by `contracts/device-detected-value.avsc`.
- Console JSONL and Kafka Avro payloads must preserve the same field meaning.
- Publication follows the same validation, consolidation, and `DeviceDeduplicationWindowMinutes`
  emission-suppression rules as console output.

## Application Model: DiscoveryValidationResult

### Purpose
Capture validation outcome before emission.

### Fields
- `IsValid` (bool)
- `Errors` (collection of messages)

### Behavior
- Invalid discovery inputs are not emitted as `DeviceDetected`.
- Processing continues for subsequent observations.

## Out-of-Scope Entities
- Entities and contracts not listed in this document are outside the device discovery data model for
  this increment.
