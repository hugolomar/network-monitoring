using System.Collections.Concurrent;
using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// In-memory graph store used by graph adapters in local/test execution.
/// </summary>
public sealed class InMemoryGraphStore
{
    private readonly ConcurrentDictionary<string, GraphNode> _nodes = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<GraphEdgeKey, GraphEdge> _edges = new();

    /// <summary>
    /// Upserts one graph node by identity.
    /// </summary>
    /// <param name="id">Node identity.</param>
    /// <param name="kind">Node kind.</param>
    public void UpsertNode(string id, string kind)
    {
        _nodes.AddOrUpdate(
            id,
            _ => new GraphNode(id, kind),
            (_, existing) => existing with { Kind = kind });
    }

    /// <summary>
    /// Upserts one communication edge keyed by source, destination, and protocol.
    /// </summary>
    /// <param name="sourceId">Source identity.</param>
    /// <param name="destinationId">Destination identity.</param>
    /// <param name="protocol">Protocol dimension.</param>
    /// <param name="detectedAtUtc">Observed timestamp.</param>
    public void UpsertEdge(string sourceId, string destinationId, string protocol, DateTimeOffset detectedAtUtc)
    {
        var key = new GraphEdgeKey(sourceId, destinationId, protocol);
        _edges.AddOrUpdate(
            key,
            _ => new GraphEdge(sourceId, destinationId, protocol, 1, detectedAtUtc, detectedAtUtc),
            (_, existing) => existing with
            {
                Weight = existing.Weight + 1,
                LastSeenUtc = detectedAtUtc > existing.LastSeenUtc ? detectedAtUtc : existing.LastSeenUtc
            });
    }

    /// <summary>
    /// Returns a bounded neighborhood from the requested root.
    /// </summary>
    /// <param name="rootIdentity">Root identity.</param>
    /// <param name="depth">Traversal depth.</param>
    /// <param name="limit">Node limit.</param>
    /// <returns>Bounded graph query result.</returns>
    public GraphQueryResult QueryNeighborhood(string rootIdentity, int depth, int limit)
    {
        if (!_nodes.ContainsKey(rootIdentity))
        {
            return new GraphQueryResult(Array.Empty<GraphNode>(), Array.Empty<GraphEdge>(), false);
        }

        var visited = new HashSet<string>(StringComparer.Ordinal) { rootIdentity };
        var queue = new Queue<(string Id, int Hop)>();
        queue.Enqueue((rootIdentity, 0));

        while (queue.Count > 0)
        {
            var (current, hop) = queue.Dequeue();
            if (hop >= depth || visited.Count >= limit)
            {
                continue;
            }

            foreach (var edge in _edges.Values.Where(e => e.SourceId == current || e.DestinationId == current))
            {
                var neighbor = edge.SourceId == current ? edge.DestinationId : edge.SourceId;
                if (visited.Count >= limit)
                {
                    break;
                }

                if (visited.Add(neighbor))
                {
                    queue.Enqueue((neighbor, hop + 1));
                }
            }
        }

        var nodes = visited
            .Select(id => _nodes.GetValueOrDefault(id))
            .Where(node => node is not null)
            .Cast<GraphNode>()
            .ToArray();

        var nodeSet = nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var edges = _edges.Values
            .Where(edge => nodeSet.Contains(edge.SourceId) && nodeSet.Contains(edge.DestinationId))
            .ToArray();

        var truncated = nodes.Length >= limit;
        return new GraphQueryResult(nodes, edges, truncated);
    }

    /// <summary>
    /// Returns a bounded graph snapshot without neighborhood filtering.
    /// </summary>
    /// <param name="limit">Maximum number of nodes to include.</param>
    /// <returns>Graph snapshot result.</returns>
    public GraphQueryResult QuerySnapshot(int limit)
    {
        var nodes = _nodes.Values
            .Take(limit)
            .ToArray();

        var nodeSet = nodes
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);

        var edges = _edges.Values
            .Where(edge => nodeSet.Contains(edge.SourceId) && nodeSet.Contains(edge.DestinationId))
            .ToArray();

        var truncated = _nodes.Count > nodes.Length;
        return new GraphQueryResult(nodes, edges, truncated);
    }

    /// <summary>
    /// Executes stale-edge and orphan-external-host cleanup.
    /// </summary>
    /// <param name="cutoffUtc">Stale-edge cutoff timestamp.</param>
    /// <returns>Retention sweep outcome.</returns>
    public GraphRetentionSweepOutcome Sweep(DateTimeOffset cutoffUtc)
    {
        var staleKeys = _edges
            .Where(pair => pair.Value.LastSeenUtc < cutoffUtc)
            .Select(pair => pair.Key)
            .ToArray();

        var staleRemoved = 0;
        foreach (var key in staleKeys)
        {
            if (_edges.TryRemove(key, out _))
            {
                staleRemoved++;
            }
        }

        var destinationsWithInbound = _edges.Values
            .Select(edge => edge.DestinationId)
            .ToHashSet(StringComparer.Ordinal);

        var orphanExternalHostsRemoved = 0;
        foreach (var node in _nodes.Values.Where(node => node.Kind == "ExternalHost"))
        {
            if (destinationsWithInbound.Contains(node.Id))
            {
                continue;
            }

            if (_nodes.TryRemove(node.Id, out _))
            {
                orphanExternalHostsRemoved++;
            }
        }

        return new GraphRetentionSweepOutcome(staleRemoved, orphanExternalHostsRemoved, DateTimeOffset.UtcNow);
    }
}

internal readonly record struct GraphEdgeKey(string SourceId, string DestinationId, string Protocol);
