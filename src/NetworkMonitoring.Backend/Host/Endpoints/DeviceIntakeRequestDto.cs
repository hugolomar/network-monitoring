namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Data transfer object representing the source event that triggered the device intake.
/// </summary>
/// <param name="EventType">The type of the event.</param>
/// <param name="Source">The source system that generated the event.</param>
/// <param name="SchemaVersion">The version of the event schema.</param>
/// <param name="OccurredAtUtc">The timestamp when the event occurred in UTC.</param>
public sealed record SourceEventDto(
    string? EventType,
    string? Source,
    int? SchemaVersion,
    DateTimeOffset? OccurredAtUtc);

/// <summary>
/// Data transfer object representing a request to intake or update a device in the inventory.
/// </summary>
/// <param name="MacAddress">The MAC address of the device.</param>
/// <param name="PrimaryIp">The primary IP address of the device.</param>
/// <param name="Hostname">The hostname of the device.</param>
/// <param name="ObservedIps">A list of all IP addresses observed for the device.</param>
/// <param name="FirstSeenUtc">The timestamp when the device was first seen in UTC.</param>
/// <param name="LastSeenUtc">The timestamp when the device was last seen in UTC.</param>
/// <param name="DiscoverySource">The source through which the device was discovered.</param>
/// <param name="SourceEvent">Details about the event that triggered this intake request.</param>
public sealed record DeviceIntakeRequestDto(
    string? MacAddress,
    string? PrimaryIp,
    string? Hostname,
    string[]? ObservedIps,
    DateTimeOffset? FirstSeenUtc,
    DateTimeOffset? LastSeenUtc,
    string? DiscoverySource,
    SourceEventDto? SourceEvent);
