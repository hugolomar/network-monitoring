using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Application.UseCases;

/// <summary>
/// Projects communication observations into graph relationships with bounded retries.
/// </summary>
public sealed class ProjectCommunicationGraphUseCase(
    IGraphProjectionRepository projectionRepository,
    IDeviceInventoryRepository inventoryRepository,
    IGraphTelemetry telemetry,
    IOptions<BackendOptions> options)
{
    /// <summary>
    /// Executes one projection write with bounded retry and diagnostics emission.
    /// </summary>
    public async Task Execute(
        string sourceIdentity,
        string destinationIdentity,
        string destinationKind,
        string protocol,
        DateTimeOffset detectedAtUtc,
        CancellationToken cancellationToken)
    {
        var graph = options.Value.Graph;
        var attempts = graph.ProjectionMaxRetries + 1;
        var delay = TimeSpan.FromMilliseconds(graph.ProjectionRetryBaseDelayMs);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                await projectionRepository.UpsertCommunication(
                    sourceIdentity,
                    destinationIdentity,
                    destinationKind,
                    protocol,
                    detectedAtUtc,
                    cancellationToken);

                telemetry.TrackProjection(new GraphProjectionDiagnostics(
                    "projection_upsert",
                    "success",
                    attempt,
                    (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
                    DateTimeOffset.UtcNow));
                return;
            }
            catch (Exception)
            {
                var latency = (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;
                if (attempt >= attempts)
                {
                    telemetry.TrackProjection(new GraphProjectionDiagnostics(
                        "projection_upsert",
                        "retry_exhausted",
                        attempt,
                        latency,
                        DateTimeOffset.UtcNow,
                        "GRAPH_PROJECTION_RETRY_EXHAUSTED"));
                    return;
                }

                telemetry.TrackProjection(new GraphProjectionDiagnostics(
                    "projection_upsert",
                    "retry",
                    attempt,
                    latency,
                    DateTimeOffset.UtcNow));

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
    }

    /// <summary>
    /// Executes a projection sweep from indexed session aggregates.
    /// </summary>
    /// <param name="records">Aggregated session records to project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteSweep(
        IReadOnlyCollection<SessionProjectionRecord> records,
        CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return;
        }

        var inventory = await inventoryRepository.List(cancellationToken);
        var identityByIp = BuildIdentityByIp(inventory);

        foreach (var record in records)
        {
            if (!identityByIp.TryGetValue(record.SourceIp, out var sourceIdentity))
            {
                continue;
            }

            var destination = ResolveDestinationIdentity(identityByIp, record.DestinationIp);

            await ExecuteAggregate(
                sourceIdentity,
                destination.DestinationIdentity,
                destination.DestinationKind,
                record.Protocol,
                record.ObservationCount,
                record.FirstSeenUtc,
                record.LastSeenUtc,
                cancellationToken);
        }
    }

    private async Task ExecuteAggregate(
        string sourceIdentity,
        string destinationIdentity,
        string destinationKind,
        string protocol,
        long weight,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        CancellationToken cancellationToken)
    {
        var graph = options.Value.Graph;
        var attempts = graph.ProjectionMaxRetries + 1;
        var delay = TimeSpan.FromMilliseconds(graph.ProjectionRetryBaseDelayMs);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                await projectionRepository.UpsertCommunicationAggregate(
                    sourceIdentity,
                    destinationIdentity,
                    destinationKind,
                    protocol,
                    weight,
                    firstSeenUtc,
                    lastSeenUtc,
                    cancellationToken);

                telemetry.TrackProjection(new GraphProjectionDiagnostics(
                    "projection_upsert_aggregate",
                    "success",
                    attempt,
                    (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
                    DateTimeOffset.UtcNow));
                return;
            }
            catch (Exception)
            {
                var latency = (long)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;
                if (attempt >= attempts)
                {
                    telemetry.TrackProjection(new GraphProjectionDiagnostics(
                        "projection_upsert_aggregate",
                        "retry_exhausted",
                        attempt,
                        latency,
                        DateTimeOffset.UtcNow,
                        "GRAPH_PROJECTION_RETRY_EXHAUSTED"));
                    return;
                }

                telemetry.TrackProjection(new GraphProjectionDiagnostics(
                    "projection_upsert_aggregate",
                    "retry",
                    attempt,
                    latency,
                    DateTimeOffset.UtcNow));

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
    }

    private static Dictionary<string, string> BuildIdentityByIp(IReadOnlyCollection<Domain.Entities.Device> inventory)
    {
        var identities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var device in inventory.Where(item => item.Id.HasValue))
        {
            var identity = $"device-{device.Id!.Value}";
            if (device.PrimaryIp is not null)
            {
                identities[device.PrimaryIp.Value] = identity;
            }

            foreach (var observed in device.ObservedIps)
            {
                identities[observed.Value] = identity;
            }
        }

        return identities;
    }

    private static (string DestinationIdentity, string DestinationKind) ResolveDestinationIdentity(
        IReadOnlyDictionary<string, string> identityByIp,
        string destinationIp)
    {
        if (identityByIp.TryGetValue(destinationIp, out var internalIdentity))
        {
            return (internalIdentity, "InternalDevice");
        }

        return (destinationIp, "ExternalHost");
    }
}
