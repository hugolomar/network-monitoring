namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Emits end-to-end freshness for device intake (FR-017).
/// </summary>
public interface IIntakeFlowTelemetry
{
    /// <summary>
    /// Records elapsed time from network observation to a queryable inventory record.
    /// </summary>
    /// <param name="freshnessMs">Elapsed milliseconds; negative values are ignored.</param>
    /// <param name="outcome">Intake outcome (created, updated, idempotent).</param>
    void TrackFreshness(long freshnessMs, string outcome);
}
