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
}
