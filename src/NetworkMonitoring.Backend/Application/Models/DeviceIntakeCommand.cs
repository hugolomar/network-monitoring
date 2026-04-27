namespace NetworkMonitoring.Backend.Application.Models;

public sealed record SourceEventMetadata(
    string? EventType,
    string? Source,
    int? SchemaVersion,
    DateTimeOffset? OccurredAtUtc);

public sealed record DeviceIntakeCommand(
    string? IdempotencyKey,
    string? MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyCollection<string>? ObservedIps,
    DateTimeOffset? FirstSeenUtc,
    DateTimeOffset? LastSeenUtc,
    string? DiscoverySource,
    SourceEventMetadata? SourceEvent);
