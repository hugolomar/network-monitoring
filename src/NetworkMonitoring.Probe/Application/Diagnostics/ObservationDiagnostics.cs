namespace NetworkMonitoring.Probe.Application.Diagnostics;

public sealed record ObservationDiagnostics(
    string Message,
    string? SourceIp,
    string? DestinationIp,
    DateTimeOffset OccurredAtUtc);
