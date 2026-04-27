namespace NetworkMonitoring.Backend.Host.Endpoints;

public sealed record SourceEventDto(
    string? EventType,
    string? Source,
    int? SchemaVersion,
    DateTimeOffset? OccurredAtUtc);

public sealed record DeviceIntakeRequestDto(
    string? MacAddress,
    string? PrimaryIp,
    string? Hostname,
    string[]? ObservedIps,
    DateTimeOffset? FirstSeenUtc,
    DateTimeOffset? LastSeenUtc,
    string? DiscoverySource,
    SourceEventDto? SourceEvent);
