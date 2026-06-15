using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Retrieves graph neighborhoods from the graph store.
/// </summary>
public interface IGraphQueryRepository
{
    /// <summary>
    /// Gets a bounded graph neighborhood for the requested root identity.
    /// </summary>
    Task<GraphQueryResult> GetDeviceGraph(
        string rootIdentity,
        int depth,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a bounded full graph snapshot without requiring a root identity.
    /// </summary>
    Task<GraphQueryResult> GetGraphSnapshot(
        int limit,
        CancellationToken cancellationToken);
}
