using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph query adapter.
/// </summary>
public sealed class Neo4jGraphQueryRepository(InMemoryGraphStore store) : IGraphQueryRepository
{
    /// <inheritdoc />
    public Task<GraphQueryResult> GetDeviceGraph(
        string rootIdentity,
        int depth,
        int limit,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(store.QueryNeighborhood(rootIdentity, depth, limit));
    }
}
