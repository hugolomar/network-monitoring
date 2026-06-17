namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Query parameters accepted by graph retrieval.
/// </summary>
/// <param name="RootDeviceId">The required root graph identity for neighborhood traversal.</param>
/// <param name="Depth">Optional traversal depth request.</param>
/// <param name="Limit">Optional maximum number of nodes to return.</param>
public sealed record GraphDevicesRequestDto(
    string RootDeviceId,
    int? Depth,
    int? Limit);

/// <summary>
/// Query parameters accepted by full-graph snapshot retrieval.
/// </summary>
/// <param name="Limit">Optional maximum number of nodes to return.</param>
public sealed record GraphSnapshotRequestDto(
    int? Limit);
