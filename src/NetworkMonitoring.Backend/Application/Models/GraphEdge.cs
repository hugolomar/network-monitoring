namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents one directed communication edge.
/// </summary>
/// <param name="SourceId">Source identity.</param>
/// <param name="DestinationId">Destination identity.</param>
/// <param name="Protocol">Protocol dimension.</param>
/// <param name="Weight">Observation count.</param>
/// <param name="FirstSeenUtc">First seen timestamp.</param>
/// <param name="LastSeenUtc">Last seen timestamp.</param>
public sealed record GraphEdge(
    string SourceId,
    string DestinationId,
    string Protocol,
    long Weight,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc);
