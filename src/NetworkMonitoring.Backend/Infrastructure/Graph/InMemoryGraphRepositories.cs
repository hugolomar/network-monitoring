using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// In-memory implementation of graph projection writes.
/// </summary>
public sealed class InMemoryGraphProjectionRepository(InMemoryGraphStore store) : IGraphProjectionRepository
{
    /// <inheritdoc />
    public Task UpsertCommunication(
        string sourceIdentity,
        string destinationIdentity,
        string destinationKind,
        string protocol,
        DateTimeOffset detectedAtUtc,
        CancellationToken cancellationToken)
    {
        store.UpsertNode(sourceIdentity, "InternalDevice");
        store.UpsertNode(destinationIdentity, destinationKind);
        store.UpsertEdge(sourceIdentity, destinationIdentity, protocol, detectedAtUtc);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of graph neighborhood queries.
/// </summary>
public sealed class InMemoryGraphQueryRepository(InMemoryGraphStore store) : IGraphQueryRepository
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

    /// <inheritdoc />
    public Task<GraphQueryResult> GetGraphSnapshot(int limit, CancellationToken cancellationToken)
    {
        return Task.FromResult(store.QuerySnapshot(limit));
    }
}

/// <summary>
/// In-memory implementation of graph retention cleanup.
/// </summary>
public sealed class InMemoryGraphRetentionRepository(InMemoryGraphStore store) : IGraphRetentionRepository
{
    /// <inheritdoc />
    public Task<GraphRetentionSweepOutcome> Sweep(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        return Task.FromResult(store.Sweep(cutoffUtc));
    }
}

