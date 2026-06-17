namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents a bounded graph neighborhood response.
/// </summary>
/// <param name="Nodes">Returned nodes.</param>
/// <param name="Edges">Returned edges among returned nodes.</param>
/// <param name="Truncated">Whether node cap truncated the result.</param>
public sealed record GraphQueryResult(
    IReadOnlyCollection<GraphNode> Nodes,
    IReadOnlyCollection<GraphEdge> Edges,
    bool Truncated);
