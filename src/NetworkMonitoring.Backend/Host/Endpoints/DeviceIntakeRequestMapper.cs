using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Host.Endpoints;

public static class DeviceIntakeRequestMapper
{
    public static DeviceIntakeCommand ToCommand(DeviceIntakeRequestDto request, string? idempotencyKey)
    {
        return new DeviceIntakeCommand(
            idempotencyKey,
            request.MacAddress,
            request.PrimaryIp,
            request.Hostname,
            request.ObservedIps ?? [],
            request.FirstSeenUtc,
            request.LastSeenUtc,
            request.DiscoverySource,
            request.SourceEvent is null
                ? null
                : new SourceEventMetadata(
                    request.SourceEvent.EventType,
                    request.SourceEvent.Source,
                    request.SourceEvent.SchemaVersion,
                    request.SourceEvent.OccurredAtUtc));
    }
}
