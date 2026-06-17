namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents one node in graph query results.
/// </summary>
/// <param name="Id">Node identity.</param>
/// <param name="Kind">Node kind (internal device or external host).</param>
public sealed record GraphNode(
    string Id,
    string Kind);
