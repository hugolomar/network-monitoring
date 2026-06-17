namespace NetworkMonitoring.IntegrationConsole.Application.Models;

/// <summary>
/// Domain model representing a device detection event received from external sources.
/// </summary>
/// <param name="EventType">Type of the event (e.g., "DeviceDetected").</param>
/// <param name="OccurredAtUtc">Timestamp when the event occurred.</param>
/// <param name="Source">The source system that generated the event.</param>
/// <param name="SchemaVersion">Version of the event schema.</param>
/// <param name="DeviceId">Optional external device identifier.</param>
/// <param name="MacAddress">MAC address of the detected device.</param>
/// <param name="PrimaryIp">Primary IP address of the device, if known.</param>
/// <param name="Hostname">Hostname of the device, if known.</param>
/// <param name="ObservedIps">List of all IPs observed for this device.</param>
/// <param name="FirstSeenUtc">Timestamp when the device was first seen.</param>
/// <param name="LastSeenUtc">Timestamp when the device was last seen.</param>
/// <param name="DiscoverySource">The discovery mechanism (e.g., "nmap", "tshark").</param>
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
