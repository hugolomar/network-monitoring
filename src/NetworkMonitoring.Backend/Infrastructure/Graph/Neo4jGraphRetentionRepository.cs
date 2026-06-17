using Neo4j.Driver;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph retention adapter.
/// </summary>
public sealed class Neo4jGraphRetentionRepository(Neo4jDriverAccessor driverAccessor) : IGraphRetentionRepository
{
    /// <inheritdoc />
    public Task<GraphRetentionSweepOutcome> Sweep(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        return SweepAsync(cutoffUtc, cancellationToken);
    }

    private async Task<GraphRetentionSweepOutcome> SweepAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        var driver = driverAccessor.GetOrCreate();
        var session = driver.AsyncSession();
        try
        {
            var staleDeleteCypher = """
                MATCH ()-[edge:COMMUNICATED_WITH]->()
                WHERE edge.lastSeenUtc < datetime($cutoffUtc)
                WITH collect(edge) AS staleEdges, count(edge) AS staleCount
                FOREACH (edge IN staleEdges | DELETE edge)
                RETURN staleCount
                """;

            var staleCursor = await session.RunAsync(staleDeleteCypher, new
            {
                cutoffUtc = cutoffUtc.UtcDateTime.ToString("O")
            });
            var staleRecord = await staleCursor.SingleAsync();
            var staleRemoved = staleRecord[0].As<int>();

            var orphanDeleteCypher = """
                MATCH (node:Device:ExternalHost)
                WHERE NOT ()-[:COMMUNICATED_WITH]->(node)
                WITH collect(node) AS orphanNodes, count(node) AS orphanCount
                FOREACH (node IN orphanNodes | DELETE node)
                RETURN orphanCount
                """;

            var orphanCursor = await session.RunAsync(orphanDeleteCypher);
            var orphanRecord = await orphanCursor.SingleAsync();
            var orphanRemoved = orphanRecord[0].As<int>();

            return new GraphRetentionSweepOutcome(staleRemoved, orphanRemoved, DateTimeOffset.UtcNow);
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}
