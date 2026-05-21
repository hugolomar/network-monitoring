namespace NetworkMonitoring.Probe.Application.Diagnostics;

/// <summary>
/// Represents diagnostic information about a traffic observation event.
/// Useful for debugging and system health monitoring.
/// </summary>
/// <param name="Message">A descriptive message about the diagnostic event.</param>
/// <param name="SourceIp">The origin IP address associated with the event (Optional).</param>
/// <param name="DestinationIp">The target IP address associated with the event (Optional).</param>
/// <param name="OccurredAtUtc">The timestamp when the event occurred.</param>
public sealed record ObservationDiagnostics(
    string Message,
    string? SourceIp,
    string? DestinationIp,
    DateTimeOffset OccurredAtUtc);
