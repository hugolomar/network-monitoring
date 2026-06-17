namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Projection write telemetry event payload.
/// </summary>
/// <param name="Operation">Operation name.</param>
/// <param name="Outcome">Outcome category.</param>
/// <param name="AttemptCount">Number of attempts performed.</param>
/// <param name="LatencyMs">Observed latency in milliseconds.</param>
/// <param name="OccurredAtUtc">Timestamp of diagnostic event.</param>
/// <param name="ErrorCode">Optional error code.</param>
public sealed record GraphProjectionDiagnostics(
    string Operation,
    string Outcome,
    int AttemptCount,
    long LatencyMs,
    DateTimeOffset OccurredAtUtc,
    string? ErrorCode = null);
