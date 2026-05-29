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
}
