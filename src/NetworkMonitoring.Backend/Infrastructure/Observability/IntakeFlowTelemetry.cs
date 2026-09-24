using System.Diagnostics.Metrics;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Observability;

/// <summary>
/// OpenTelemetry-backed intake freshness metrics.
/// </summary>
public sealed class IntakeFlowTelemetry : IIntakeFlowTelemetry
{
    private static readonly Meter Meter = new("NetworkMonitoring.Backend.Intake");
    private static readonly Histogram<long> Freshness = Meter.CreateHistogram<long>("observation_to_inventory_freshness_ms");

    /// <inheritdoc />
    public void TrackFreshness(long freshnessMs, string outcome)
    {
        if (freshnessMs < 0)
        {
            return;
        }

        Freshness.Record(freshnessMs, new KeyValuePair<string, object?>("outcome", outcome));
    }
}
