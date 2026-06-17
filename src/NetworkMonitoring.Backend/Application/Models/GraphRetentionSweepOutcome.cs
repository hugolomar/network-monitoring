namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents one retention sweep result.
/// </summary>
/// <param name="StaleRelationshipsRemoved">Number of stale relationships removed.</param>
/// <param name="OrphanExternalHostsRemoved">Number of orphan external hosts removed.</param>
/// <param name="ExecutedAtUtc">Execution timestamp.</param>
public sealed record GraphRetentionSweepOutcome(
    int StaleRelationshipsRemoved,
    int OrphanExternalHostsRemoved,
    DateTimeOffset ExecutedAtUtc);
