namespace NetworkMonitoring.IntegrationConsole.Application.Models;

public sealed record DeviceDetectedEvent(
    string EventType,
    DateTimeOffset OccurredAtUtc,
    string Source,
    int SchemaVersion,
    int? DeviceId,
    string MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyList<string> ObservedIps,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    string DiscoverySource);
