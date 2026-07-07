using System.Text.Json;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Provides JSON serialization for domain entities intended for console output.
/// This serializer ensures a consistent JSON format for terminal-based observability.
/// </summary>
public sealed class ConsoleRecordSerializer
{
    /// <summary>
    /// Serializes a <see cref="Session"/> into a JSON string.
    /// </summary>
    /// <param name="session">The session to serialize.</param>
    /// <returns>A JSON string representation of the session detection event.</returns>
    public string SerializeSession(Session session)
    {
        var payload = new
        {
            eventType = "SessionDetected",
            occurredAtUtc = session.LastSeenUtc,
            source = "probe",
            schemaVersion = 1,
            sessionId = session.Id,
            sourceIp = session.SourceIp.Value,
            destinationIp = session.DestinationIp.Value,
            sourcePort = session.SourcePort?.Value,
            destinationPort = session.DestinationPort?.Value,
            protocol = session.Protocol.Value,
            firstSeenUtc = session.FirstSeenUtc,
            lastSeenUtc = session.LastSeenUtc,
            bytesObserved = session.BytesObserved
        };

        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Serializes a <see cref="Device"/> into a JSON string.
    /// </summary>
    /// <param name="device">The device to serialize.</param>
    /// <returns>A JSON string representation of the device detection event.</returns>
    public string SerializeDevice(Device device)
    {
        var payload = new
        {
            eventType = "DeviceDetected",
            occurredAtUtc = device.LastSeenUtc,
            source = "probe",
            schemaVersion = 1,
            deviceId = device.Id,
            macAddress = device.MacAddress.Value,
            primaryIp = device.PrimaryIp?.Value,
            hostname = device.Hostname,
            observedIps = device.ObservedIps.Select(x => x.Value).OrderBy(value => value).ToArray(),
            firstSeenUtc = device.FirstSeenUtc,
            lastSeenUtc = device.LastSeenUtc,
            discoverySource = device.DiscoverySource.Value
        };

        return JsonSerializer.Serialize(payload);
    }
}
