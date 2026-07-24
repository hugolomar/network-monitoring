using NetworkMonitoring.Backend.Application.Models;

namespace NetworkMonitoring.Backend.Application.Services;

/// <summary>
/// Evaluates critical-flow objective breaches and builds actionable alert payloads.
/// </summary>
public sealed class CriticalFlowObjectiveEvaluator
{
    private const int DefaultRetentionBreachThreshold = 50;

    /// <summary>
    /// Evaluates one retention outcome and returns an alert payload when objective breach is detected.
    /// </summary>
    /// <param name="outcome">Retention sweep diagnostics used as baseline degradation signal.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="traceId">Optional distributed trace identifier.</param>
    /// <returns>An actionable alert payload when breach criteria are met; otherwise <see langword="null"/>.</returns>
    public OperationalAlertPayload? EvaluateRetentionDegradation(
        GraphRetentionSweepOutcome outcome,
        string? correlationId = null,
        string? traceId = null)
    {
        if (outcome.StaleRelationshipsRemoved < DefaultRetentionBreachThreshold)
        {
            return null;
        }

        var reason = $"Retention sweep removed {outcome.StaleRelationshipsRemoved} stale relationships (threshold={DefaultRetentionBreachThreshold}).";
        return new OperationalAlertPayload(
            FlowId: "graph-retention",
            BreachReason: reason,
            TriggeredAtUtc: outcome.ExecutedAtUtc,
            ImpactedComponent: "NetworkMonitoring.Backend.GraphRetention",
            Severity: "warning",
            CorrelationId: correlationId,
            TraceId: traceId);
    }
}
