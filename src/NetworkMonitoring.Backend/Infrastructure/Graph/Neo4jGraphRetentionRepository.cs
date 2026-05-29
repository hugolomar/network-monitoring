using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph retention adapter.
/// </summary>
public sealed class Neo4jGraphRetentionRepository(InMemoryGraphStore store) : IGraphRetentionRepository
{
    /// <inheritdoc />
    public Task<GraphRetentionSweepOutcome> Sweep(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        return Task.FromResult(store.Sweep(cutoffUtc));
    }
}
