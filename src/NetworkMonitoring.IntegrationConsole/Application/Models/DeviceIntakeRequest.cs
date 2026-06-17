namespace NetworkMonitoring.IntegrationConsole.Application.Models;

/// <summary>
/// Request model for the Backend API device intake endpoint.
/// </summary>
/// <param name="MacAddress">MAC address of the device.</param>
/// <param name="PrimaryIp">Primary IP address.</param>
/// <param name="Hostname">Device hostname.</param>
/// <param name="ObservedIps">All observed IPs.</param>
/// <param name="FirstSeenUtc">First seen timestamp.</param>
/// <param name="LastSeenUtc">Last seen timestamp.</param>
/// <param name="DiscoverySource">Discovery source.</param>
/// <param name="SourceEvent">Metadata about the original source event.</param>
public sealed record DeviceIntakeRequest(
    string MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyList<string> ObservedIps,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    string DiscoverySource,
    SourceEventMetadata SourceEvent);

/// <summary>
/// Metadata about the source event that triggered the intake.
/// </summary>
/// <param name="EventType">Original event type.</param>
/// <param name="Source">Original event source.</param>
/// <param name="SchemaVersion">Original schema version.</param>
/// <param name="OccurredAtUtc">When the original event occurred.</param>
public sealed record SourceEventMetadata(
    string EventType,
    string Source,
    int SchemaVersion,
    DateTimeOffset OccurredAtUtc);
