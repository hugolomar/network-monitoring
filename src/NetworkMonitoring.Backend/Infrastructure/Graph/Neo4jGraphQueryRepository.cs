using Neo4j.Driver;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Graph query adapter.
/// </summary>
public sealed class Neo4jGraphQueryRepository(Neo4jDriverAccessor driverAccessor) : IGraphQueryRepository
{
    /// <inheritdoc />
    public Task<GraphQueryResult> GetDeviceGraph(
        string rootIdentity,
        int depth,
        int limit,
        CancellationToken cancellationToken)
    {
        return QueryAsync(rootIdentity, depth, limit, cancellationToken);
    }

    /// <inheritdoc />
    public Task<GraphQueryResult> GetGraphSnapshot(int limit, CancellationToken cancellationToken)
    {
        return SnapshotAsync(limit, cancellationToken);
    }

    private async Task<GraphQueryResult> QueryAsync(
        string rootIdentity,
        int depth,
        int limit,
        CancellationToken cancellationToken)
    {
        var driver = driverAccessor.GetOrCreate();
        var session = driver.AsyncSession();
        try
        {
            // Query undirected neighborhood bounded by hop depth.
            var cypher = """
                MATCH (root:Device {id: $rootIdentity})
                OPTIONAL MATCH path = (root)-[:COMMUNICATED_WITH*1..3]-(neighbor:Device)
                WHERE path IS NULL OR length(path) <= $depth
                WITH root, collect(path) AS rawPaths
                WITH root, [p IN rawPaths WHERE p IS NOT NULL] AS paths
                UNWIND CASE WHEN size(paths) = 0 THEN [null] ELSE paths END AS p
                WITH root, paths, p
                OPTIONAL MATCH (n) WHERE p IS NOT NULL AND n IN nodes(p)
                WITH root, paths, collect(DISTINCT n) AS collectedNodes
                WITH (CASE WHEN size(collectedNodes) = 0 THEN [root] ELSE collectedNodes + [root] END)[..$limit] AS limitedNodes, paths
                WITH limitedNodes, [p IN paths WHERE all(node IN nodes(p) WHERE node IN limitedNodes)] AS limitedPaths
                RETURN limitedNodes AS nodes, limitedPaths AS paths
                """;

            var cursor = await session.RunAsync(cypher, new
            {
                rootIdentity,
                depth,
                limit
            });

            var record = await cursor.SingleOrDefaultAsync();
            if (record is null)
            {
                return new GraphQueryResult(Array.Empty<GraphNode>(), Array.Empty<GraphEdge>(), false);
            }

            var nodeValues = record["nodes"]
                .As<List<object>>()
                .OfType<INode>()
                .ToList();

            var pathValues = record["paths"]
                .As<List<object>>()
                .OfType<IPath>()
                .ToList();

            var nodes = nodeValues
                .Select(n => new GraphNode(
                    n.Properties.TryGetValue("id", out var id) ? id?.ToString() ?? string.Empty : string.Empty,
                    n.Properties.TryGetValue("kind", out var kind) ? kind?.ToString() ?? "InternalDevice" : "InternalDevice"))
                .Where(n => !string.IsNullOrWhiteSpace(n.Id))
                .DistinctBy(n => n.Id)
                .ToArray();

            var edges = pathValues
                .SelectMany(path => path.Relationships)
                .Select(rel => new GraphEdge(
                    rel.StartNodeElementId is not null
                        ? nodeValues.FirstOrDefault(n => n.ElementId == rel.StartNodeElementId)?.Properties["id"]?.ToString() ?? string.Empty
                        : string.Empty,
                    rel.EndNodeElementId is not null
                        ? nodeValues.FirstOrDefault(n => n.ElementId == rel.EndNodeElementId)?.Properties["id"]?.ToString() ?? string.Empty
                        : string.Empty,
                    rel.Properties.TryGetValue("protocol", out var protocol) ? protocol?.ToString() ?? "UNKNOWN" : "UNKNOWN",
                    rel.Properties.TryGetValue("weight", out var weight) ? Convert.ToInt64(weight) : 1L,
                    rel.Properties.TryGetValue("firstSeenUtc", out var firstSeen)
                        ? ConvertToDateTimeOffset(firstSeen)
                        : DateTimeOffset.UtcNow,
                    rel.Properties.TryGetValue("lastSeenUtc", out var lastSeen)
                        ? ConvertToDateTimeOffset(lastSeen)
                        : DateTimeOffset.UtcNow))
                .Where(e => !string.IsNullOrWhiteSpace(e.SourceId) && !string.IsNullOrWhiteSpace(e.DestinationId))
                .DistinctBy(e => $"{e.SourceId}|{e.DestinationId}|{e.Protocol}")
                .ToArray();

            var truncated = nodes.Length >= limit;
            return new GraphQueryResult(nodes, edges, truncated);
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private async Task<GraphQueryResult> SnapshotAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var driver = driverAccessor.GetOrCreate();
        var session = driver.AsyncSession();
        try
        {
            var cypher = """
                MATCH (n:Device)
                WITH collect(n)[..$limit] AS limitedNodes
                OPTIONAL MATCH p = (a:Device)-[:COMMUNICATED_WITH]->(b:Device)
                WHERE a IN limitedNodes AND b IN limitedNodes
                RETURN limitedNodes AS nodes, collect(p) AS paths
                """;

            var cursor = await session.RunAsync(cypher, new
            {
                limit
            });

            var record = await cursor.SingleOrDefaultAsync();
            if (record is null)
            {
                return new GraphQueryResult(Array.Empty<GraphNode>(), Array.Empty<GraphEdge>(), false);
            }

            var nodeValues = record["nodes"]
                .As<List<object>>()
                .OfType<INode>()
                .ToList();

            var pathValues = record["paths"]
                .As<List<object>>()
                .OfType<IPath>()
                .ToList();

            var nodes = nodeValues
                .Select(n => new GraphNode(
                    n.Properties.TryGetValue("id", out var id) ? id?.ToString() ?? string.Empty : string.Empty,
                    n.Properties.TryGetValue("kind", out var kind) ? kind?.ToString() ?? "InternalDevice" : "InternalDevice"))
                .Where(n => !string.IsNullOrWhiteSpace(n.Id))
                .DistinctBy(n => n.Id)
                .ToArray();

            var edges = pathValues
                .SelectMany(path => path.Relationships)
                .Select(rel => new GraphEdge(
                    rel.StartNodeElementId is not null
                        ? nodeValues.FirstOrDefault(n => n.ElementId == rel.StartNodeElementId)?.Properties["id"]?.ToString() ?? string.Empty
                        : string.Empty,
                    rel.EndNodeElementId is not null
                        ? nodeValues.FirstOrDefault(n => n.ElementId == rel.EndNodeElementId)?.Properties["id"]?.ToString() ?? string.Empty
                        : string.Empty,
                    rel.Properties.TryGetValue("protocol", out var protocol) ? protocol?.ToString() ?? "UNKNOWN" : "UNKNOWN",
                    rel.Properties.TryGetValue("weight", out var weight) ? Convert.ToInt64(weight) : 1L,
                    rel.Properties.TryGetValue("firstSeenUtc", out var firstSeen)
                        ? ConvertToDateTimeOffset(firstSeen)
                        : DateTimeOffset.UtcNow,
                    rel.Properties.TryGetValue("lastSeenUtc", out var lastSeen)
                        ? ConvertToDateTimeOffset(lastSeen)
                        : DateTimeOffset.UtcNow))
                .Where(e => !string.IsNullOrWhiteSpace(e.SourceId) && !string.IsNullOrWhiteSpace(e.DestinationId))
                .DistinctBy(e => $"{e.SourceId}|{e.DestinationId}|{e.Protocol}")
                .ToArray();

            var truncated = nodes.Length >= limit;
            return new GraphQueryResult(nodes, edges, truncated);
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private static DateTimeOffset ConvertToDateTimeOffset(object? value)
    {
        if (value is DateTimeOffset dto)
        {
            return dto;
        }

        if (value is DateTime dt)
        {
            return new DateTimeOffset(dt.ToUniversalTime());
        }

        if (DateTimeOffset.TryParse(value?.ToString(), out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.UtcNow;
    }
}
