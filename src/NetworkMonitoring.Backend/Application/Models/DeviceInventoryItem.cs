namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents a device in the inventory.
/// </summary>
/// <param name="Id">The unique identifier of the device.</param>
/// <param name="MacAddress">The MAC address of the device.</param>
/// <param name="PrimaryIp">The primary IP address of the device.</param>
/// <param name="Hostname">The hostname of the device.</param>
/// <param name="ObservedIps">A collection of all IP addresses observed for the device.</param>
/// <param name="FirstSeenUtc">The timestamp when the device was first seen in UTC.</param>
/// <param name="LastSeenUtc">The timestamp when the device was last seen in UTC.</param>
/// <param name="DiscoverySource">The source that discovered the device.</param>
public sealed record DeviceInventoryItem(
    int Id,
    string MacAddress,
    string? PrimaryIp,
    string? Hostname,
    IReadOnlyCollection<string> ObservedIps,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc,
    string DiscoverySource);
