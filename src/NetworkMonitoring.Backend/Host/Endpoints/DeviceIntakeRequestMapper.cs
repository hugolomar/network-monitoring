using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Provides mapping functionality from <see cref="DeviceIntakeRequestDto"/> to <see cref="DeviceIntakeCommand"/>.
/// </summary>
public static class DeviceIntakeRequestMapper
{
    /// <summary>
    /// Maps a <see cref="DeviceIntakeRequestDto"/> to a <see cref="DeviceIntakeCommand"/>.
    /// </summary>
    /// <param name="request">The request DTO to map.</param>
    /// <param name="idempotencyKey">An optional idempotency key for the command.</param>
    /// <returns>A <see cref="DeviceIntakeCommand"/> populated with data from the request.</returns>
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
