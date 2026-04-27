namespace NetworkMonitoring.Backend.Application.Models;

public sealed record DeviceInventoryItem(
    int Id,
    string MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyCollection<string> ObservedIps,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    string DiscoverySource);
