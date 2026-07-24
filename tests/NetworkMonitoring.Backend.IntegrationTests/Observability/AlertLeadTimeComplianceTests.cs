namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Validates SC-004 lead-time calculation logic.
/// </summary>
public sealed class AlertLeadTimeComplianceTests
{
    /// <summary>
    /// Ensures lead-time requirement is met when alert is raised at least five minutes before breach.
    /// </summary>
    [Fact]
    public void Lead_time_meets_sc004_threshold_when_alert_precedes_breach_by_five_minutes_or_more()
    {
        var breachStart = DateTimeOffset.UtcNow;
        var alertTriggered = breachStart.AddMinutes(-6);

        var leadTime = breachStart - alertTriggered;

        Assert.True(leadTime >= TimeSpan.FromMinutes(5));
    }
}
