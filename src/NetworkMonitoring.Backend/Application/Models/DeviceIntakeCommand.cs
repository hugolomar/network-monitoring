namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents metadata about the source event that triggered the device intake.
/// </summary>
/// <param name="EventType">The type of the event.</param>
/// <param name="Source">The source of the event.</param>
/// <param name="SchemaVersion">The schema version of the event.</param>
/// <param name="OccurredAtUtc">The timestamp when the event occurred in UTC.</param>
public sealed record SourceEventMetadata(
    string? EventType,
    string? Source,
    int? SchemaVersion,
    DateTimeOffset? OccurredAtUtc);

/// <summary>
/// Represents a command to process a device intake request.
/// </summary>
/// <param name="IdempotencyKey">A unique key to ensure idempotent processing of the intake.</param>
/// <param name="MacAddress">The MAC address of the device.</param>
/// <param name="PrimaryIp">The primary IP address of the device.</param>
/// <param name="Hostname">The hostname of the device.</param>
/// <param name="ObservedIps">A collection of all IP addresses observed for the device.</param>
/// <param name="FirstSeenUtc">The timestamp when the device was first seen in UTC.</param>
/// <param name="LastSeenUtc">The timestamp when the device was last seen in UTC.</param>
/// <param name="DiscoverySource">The source that discovered the device.</param>
/// <param name="SourceEvent">Optional metadata about the source event.</param>
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
