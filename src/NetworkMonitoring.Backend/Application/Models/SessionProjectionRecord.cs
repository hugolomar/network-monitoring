namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Aggregated session observation used to project communication relationships.
/// </summary>
/// <param name="SourceIp">Source endpoint IP.</param>
/// <param name="DestinationIp">Destination endpoint IP.</param>
/// <param name="Protocol">Transport protocol.</param>
/// <param name="ObservationCount">Number of indexed observations for the aggregation bucket.</param>
/// <param name="FirstSeenUtc">Earliest observation timestamp for the bucket.</param>
/// <param name="LastSeenUtc">Latest observation timestamp for the bucket.</param>
public sealed record SessionProjectionRecord(
    string SourceIp,
    string DestinationIp,
    string Protocol,
    long ObservationCount,
    DateTimeOffset FirstSeenUtc,
    DateTimeOffset LastSeenUtc);
