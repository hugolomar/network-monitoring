using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;

/// <summary>
/// Mapper to convert domain events into Backend API request DTOs.
/// </summary>
public static class DeviceIntakeRequestMapper
{
    /// <summary>
    /// Maps a <see cref="DeviceDetectedEvent"/> to a <see cref="DeviceIntakeRequest"/>.
    /// </summary>
    /// <param name="detectedEvent">The domain event.</param>
    /// <returns>A request model suitable for the Backend API.</returns>
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
