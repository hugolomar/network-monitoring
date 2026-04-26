using System.Text.Json;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public sealed class ConsoleRecordSerializer
{
    public string SerializeSession(Session session)
    {
        var payload = new
        {
            eventType = "SessionDetected",
            occurredAtUtc = DateTimeOffset.UtcNow,
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

    public string SerializeDevice(Device device)
    {
        var payload = new
        {
            eventType = "DeviceDetected",
            occurredAtUtc = DateTimeOffset.UtcNow,
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
