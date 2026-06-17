namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// API response payload for graph neighborhood queries.
/// </summary>
public sealed record GraphDevicesResponseDto(
    IReadOnlyCollection<GraphNodeDto> Nodes,
    IReadOnlyCollection<GraphEdgeDto> Edges,
    bool Truncated);

/// <summary>
/// Graph node DTO.
/// </summary>
public sealed record GraphNodeDto(
    string Id,
    string Kind);

/// <summary>
/// Graph edge DTO.
/// </summary>
public sealed record GraphEdgeDto(
    string SourceId,
    string DestinationId,
    string Protocol,
    long Weight,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc);
