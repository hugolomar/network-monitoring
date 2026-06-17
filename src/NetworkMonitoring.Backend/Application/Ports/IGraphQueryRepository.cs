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
    /// <param name="rootIdentity">The root graph identity used as traversal origin.</param>
    /// <param name="depth">The requested traversal depth.</param>
    /// <param name="limit">The maximum number of nodes allowed in the response.</param>
    /// <param name="cancellationToken">The cancellation token that aborts the query.</param>
    /// <returns>A bounded graph neighborhood result.</returns>
    Task<GraphQueryResult> GetDeviceGraph(
        string rootIdentity,
        int depth,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a bounded full graph snapshot without requiring a root identity.
    /// </summary>
    /// <param name="limit">The maximum number of nodes allowed in the snapshot result.</param>
    /// <param name="cancellationToken">The cancellation token that aborts the query.</param>
    /// <returns>A bounded graph snapshot result.</returns>
    Task<GraphQueryResult> GetGraphSnapshot(
        int limit,
        CancellationToken cancellationToken);
}
