namespace NetworkMonitoring.Probe.Application.Models;

/// <summary>
/// Represents a raw observation of network traffic captured at the source.
/// This model acts as a bridge between raw capture data (e.g., tshark) and domain entities.
/// </summary>
/// <param name="SourceIp">The origin IP address.</param>
/// <param name="DestinationIp">The target IP address.</param>
/// <param name="SourcePort">The transport layer source port (Optional).</param>
/// <param name="DestinationPort">The transport layer destination port (Optional).</param>
/// <param name="Protocol">The transport protocol identified.</param>
/// <param name="ObservedAtUtc">The timestamp when the traffic was captured.</param>
/// <param name="BytesObserved">The size of the captured traffic in bytes.</param>
/// <param name="SourceMac">The physical MAC address of the source (Optional).</param>
/// <param name="DestinationMac">The physical MAC address of the target (Optional).</param>
/// <param name="Hostname">The hostname associated with the source, if resolved (Optional).</param>
/// <param name="DiscoverySource">The name of the probe or source that captured the data.</param>
public sealed record TrafficObservation(
    string SourceIp,
    string DestinationIp,
    int? SourcePort,
    int? DestinationPort,
    string Protocol,
    DateTimeOffset ObservedAtUtc,
    long BytesObserved,
    string? SourceMac,
    string? DestinationMac,
    string? Hostname,
    string? DiscoverySource);
