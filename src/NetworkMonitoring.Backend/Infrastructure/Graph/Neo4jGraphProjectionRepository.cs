using Neo4j.Driver;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph projection write adapter.
/// </summary>
public sealed class Neo4jGraphProjectionRepository(Neo4jDriverAccessor driverAccessor) : IGraphProjectionRepository
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
        var cypher = """
            MERGE (source:Device {id: $sourceIdentity})
            ON CREATE SET source.kind = 'InternalDevice'
            SET source.kind = 'InternalDevice'
            MERGE (destination:Device {id: $destinationIdentity})
            ON CREATE SET destination.kind = $destinationKind
            SET destination.kind = $destinationKind
            MERGE (source)-[edge:COMMUNICATED_WITH {protocol: $protocol}]->(destination)
            ON CREATE SET edge.weight = 1,
                          edge.firstSeenUtc = datetime($detectedAtUtc),
                          edge.lastSeenUtc = datetime($detectedAtUtc)
            ON MATCH SET edge.weight = coalesce(edge.weight, 0) + 1,
                         edge.lastSeenUtc = CASE
                             WHEN edge.lastSeenUtc < datetime($detectedAtUtc) THEN datetime($detectedAtUtc)
                             ELSE edge.lastSeenUtc
                         END
            """;

        return WriteAsync(cypher, new
        {
            sourceIdentity,
            destinationIdentity,
            destinationKind,
            protocol,
            detectedAtUtc = detectedAtUtc.UtcDateTime.ToString("O")
        }, cancellationToken);
    }

    private async Task WriteAsync(string cypher, object parameters, CancellationToken cancellationToken)
    {
        var driver = driverAccessor.GetOrCreate();
        var session = driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                _ = await tx.RunAsync(cypher, parameters);
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}
