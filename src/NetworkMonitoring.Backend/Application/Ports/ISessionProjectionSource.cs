using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Provides aggregated session records used to populate the communication graph.
/// </summary>
public interface ISessionProjectionSource
{
    /// <summary>
    /// Returns aggregated session records for the requested time window.
    /// </summary>
    /// <param name="fromUtc">Inclusive start timestamp.</param>
    /// <param name="toUtc">Exclusive end timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated projection records.</returns>
    Task<IReadOnlyCollection<SessionProjectionRecord>> GetAggregatedSessions(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken);
}
