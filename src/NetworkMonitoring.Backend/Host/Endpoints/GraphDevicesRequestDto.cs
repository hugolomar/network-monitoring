namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Query parameters accepted by graph retrieval.
/// </summary>
public sealed record GraphDevicesRequestDto(
    string RootDeviceId,
    int? Depth,
    int? Limit);
