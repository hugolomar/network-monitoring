using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Default no-op telemetry bridge for graph flows.
/// </summary>
public sealed class NullGraphTelemetry : IGraphTelemetry
{
    /// <inheritdoc />
    public void TrackProjection(GraphProjectionDiagnostics diagnostics)
    {
    }

    /// <inheritdoc />
    public void TrackRetention(GraphRetentionSweepOutcome outcome)
    {
    }
}
