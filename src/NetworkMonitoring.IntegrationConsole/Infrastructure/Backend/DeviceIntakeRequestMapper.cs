using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

public static class DeviceIntakeRequestMapper
{
    public static DeviceIntakeRequest Map(DeviceDetectedEvent detectedEvent) =>
        new(
            detectedEvent.MacAddress,
            detectedEvent.PrimaryIp,
            detectedEvent.Hostname,
            detectedEvent.ObservedIps,
            detectedEvent.FirstSeenUtc,
            detectedEvent.LastSeenUtc,
            detectedEvent.DiscoverySource,
            new SourceEventMetadata(
                detectedEvent.EventType,
                detectedEvent.Source,
                detectedEvent.SchemaVersion,
                detectedEvent.OccurredAtUtc));
}
