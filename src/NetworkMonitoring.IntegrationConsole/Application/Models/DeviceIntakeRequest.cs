namespace NetworkMonitoring.IntegrationConsole.Application.Models;

public sealed record DeviceIntakeRequest(
    string MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyList<string> ObservedIps,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    string DiscoverySource,
    SourceEventMetadata SourceEvent);

public sealed record SourceEventMetadata(
    string EventType,
    string Source,
    int SchemaVersion,
    DateTimeOffset OccurredAtUtc);
