namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Query parameters accepted by graph retrieval.
/// </summary>
public sealed record GraphDevicesRequestDto(
    string RootDeviceId,
    int? Depth,
    int? Limit);

/// <summary>
/// Query parameters accepted by full-graph snapshot retrieval.
/// </summary>
public sealed record GraphSnapshotRequestDto(
    int? Limit);
