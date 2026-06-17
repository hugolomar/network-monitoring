using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Emits projection and retention telemetry.
/// </summary>
public interface IGraphTelemetry
{
    /// <summary>
    /// Emits projection diagnostic event.
    /// </summary>
    void TrackProjection(GraphProjectionDiagnostics diagnostics);

    /// <summary>
    /// Emits retention sweep outcome event.
    /// </summary>
    void TrackRetention(GraphRetentionSweepOutcome outcome);
}
