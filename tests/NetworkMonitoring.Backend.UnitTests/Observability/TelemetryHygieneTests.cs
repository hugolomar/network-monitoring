using NetworkMonitoring.Backend.Host.Telemetry;

namespace NetworkMonitoring.Backend.UnitTests.Observability;

/// <summary>
/// Verifies telemetry redaction policy behavior.
/// </summary>
public sealed class TelemetryHygieneTests
{
    /// <summary>
    /// Ensures secrets and passwords are masked before telemetry emission.
    /// </summary>
    [Fact]
    public void Redaction_masks_sensitive_fragments()
    {
        var redacted = TelemetryRedaction.Redact("token=abc123 password=hello api_key:abcd");

        Assert.DoesNotContain("abc123", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hello", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("***", redacted);
    }
}
