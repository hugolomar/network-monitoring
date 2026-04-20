namespace NetworkMonitoring.Probe.Application.Models;

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
