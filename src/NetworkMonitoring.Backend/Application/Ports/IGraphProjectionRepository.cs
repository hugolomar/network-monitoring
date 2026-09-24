namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Writes communication observations into the graph store.
/// </summary>
public interface IGraphProjectionRepository
{
    /// <summary>
    /// Upserts one communication observation.
    /// </summary>
    Task UpsertCommunication(
        string sourceIdentity,
        string destinationIdentity,
        string destinationKind,
        string protocol,
        DateTimeOffset detectedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Upserts one aggregated communication relationship using absolute counters/timestamps.
    /// </summary>
    Task UpsertCommunicationAggregate(
        string sourceIdentity,
        string destinationIdentity,
        string destinationKind,
        string protocol,
        long weight,
        DateTimeOffset firstSeenUtc,
        DateTimeOffset lastSeenUtc,
        CancellationToken cancellationToken);
}
