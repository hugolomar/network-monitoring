using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Executes retention cleanup against the graph store.
/// </summary>
public interface IGraphRetentionRepository
{
    /// <summary>
    /// Removes stale relationships and orphan external hosts.
    /// </summary>
    Task<GraphRetentionSweepOutcome> Sweep(
        DateTimeOffset cutoffUtc,
        CancellationToken cancellationToken);
}
