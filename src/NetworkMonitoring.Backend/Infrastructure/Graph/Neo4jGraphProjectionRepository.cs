using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph projection write adapter.
/// </summary>
public sealed class Neo4jGraphProjectionRepository(InMemoryGraphStore store) : IGraphProjectionRepository
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
