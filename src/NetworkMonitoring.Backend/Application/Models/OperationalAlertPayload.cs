namespace NetworkMonitoring.Backend.Application.Models;

/// <summary>
/// Represents a baseline operational alert payload for observability workflows.
/// </summary>
/// <param name="FlowId">Critical flow identifier tied to the alert.</param>
/// <param name="BreachReason">Reason that explains objective breach.</param>
/// <param name="TriggeredAtUtc">UTC timestamp when the alert was triggered.</param>
/// <param name="ImpactedComponent">Service or component impacted by the breach.</param>
/// <param name="Severity">Severity level aligned with baseline alert policy.</param>
/// <param name="CorrelationId">Correlation identifier when available.</param>
/// <param name="TraceId">Trace identifier when available.</param>
public sealed record OperationalAlertPayload(
    string FlowId,
    string BreachReason,
    DateTimeOffset TriggeredAtUtc,
    string ImpactedComponent,
    string Severity,
    string? CorrelationId,
    string? TraceId);
