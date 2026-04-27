# Contract: DeviceDetected Consumer

## Purpose

Define how the Integration Console consumes the `DeviceDetected` event stream produced by
`003-device-discovery`.

## Input Topic

- Default topic: `devices.detected`
- Configurable per deployment.

## Value Contract

- Schema Registry subject: `devices.detected-value`
- Event type: `DeviceDetected`
- The value schema is owned by `003-device-discovery`.
- This feature MUST NOT alter the value schema.

## Message Key

- Type: string
- Value: normalized MAC address.
- The key MUST match payload field `macAddress` after normalization.

## Consumer Behavior

- Valid events are mapped to `POST /devices`.
- Key/payload identity mismatch is a terminal rejection.
- Deserialization failure is a rejected/poison event with structured diagnostics.
- The consumer must continue processing later valid events after malformed or rejected events.
- Processing position should advance only after the event is classified as succeeded, rejected, or
  retry-exhausted according to the configured policy.

## Required Configuration

- Kafka bootstrap servers.
- Schema Registry URL.
- Device topic.
- Consumer group id.
- Kafka/Schema Registry security settings.
- Retry/backoff settings for downstream HTTP forwarding.
- Backend base URL.
