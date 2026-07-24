using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;
using System.Diagnostics.Metrics;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// OpenTelemetry-backed graph telemetry bridge for projection and retention flows.
/// </summary>
public sealed class NullGraphTelemetry : IGraphTelemetry
{
    private static readonly Meter Meter = new("NetworkMonitoring.Backend.Graph");
    private static readonly Counter<long> ProjectionCounter = Meter.CreateCounter<long>("graph_projection_total");
    private static readonly Histogram<long> ProjectionLatency = Meter.CreateHistogram<long>("graph_projection_latency_ms");
    private static readonly Counter<long> RetentionRemoved = Meter.CreateCounter<long>("graph_retention_removed_total");

    /// <inheritdoc />
    public void TrackProjection(GraphProjectionDiagnostics diagnostics)
    {
        ProjectionCounter.Add(1,
            new KeyValuePair<string, object?>("operation", diagnostics.Operation),
            new KeyValuePair<string, object?>("outcome", diagnostics.Outcome));
        ProjectionLatency.Record(diagnostics.LatencyMs,
            new KeyValuePair<string, object?>("operation", diagnostics.Operation),
            new KeyValuePair<string, object?>("outcome", diagnostics.Outcome));
    }

    /// <inheritdoc />
    public void TrackRetention(GraphRetentionSweepOutcome outcome)
    {
        RetentionRemoved.Add(outcome.StaleRelationshipsRemoved,
            new KeyValuePair<string, object?>("kind", "stale_relationship"));
        RetentionRemoved.Add(outcome.OrphanExternalHostsRemoved,
            new KeyValuePair<string, object?>("kind", "orphan_external_host"));
    }
}
