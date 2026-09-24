using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Services;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Verifies critical-flow alert payload generation contract.
/// </summary>
public sealed class CriticalFlowAlertingTests
{
    /// <summary>
    /// Ensures evaluator generates actionable payload when retention breach threshold is exceeded.
    /// </summary>
    [Fact]
    public void Evaluator_generates_actionable_alert_payload_on_breach()
    {
        var evaluator = new CriticalFlowObjectiveEvaluator();
        var outcome = new GraphRetentionSweepOutcome(
            StaleRelationshipsRemoved: 120,
            OrphanExternalHostsRemoved: 4,
            ExecutedAtUtc: DateTimeOffset.UtcNow);

        var payload = evaluator.EvaluateRetentionDegradation(outcome, "corr-1", "trace-1");

        Assert.NotNull(payload);
        Assert.Equal("graph-retention", payload!.FlowId);
        Assert.Contains("threshold", payload.BreachReason, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("corr-1", payload.CorrelationId);
        Assert.Equal("trace-1", payload.TraceId);
    }
}
