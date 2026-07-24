namespace NetworkMonitoring.Backend.UnitTests.Observability;

/// <summary>
/// Verifies CI gate script includes mandatory observability baseline checks.
/// </summary>
public sealed class ObservabilityGatePolicyTests
{
    /// <summary>
    /// Ensures gate script validates required observability assets.
    /// </summary>
    [Fact]
    public void Gate_script_references_required_observability_files()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
        var scriptPath = Path.Combine(root, "infrastructure/ci/check-observability-baseline.sh");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("otel-collector-config.yml", script, StringComparison.Ordinal);
        Assert.Contains("prometheus.yml", script, StringComparison.Ordinal);
        Assert.Contains("alert-rules.yml", script, StringComparison.Ordinal);
        Assert.Contains("datasources.yml", script, StringComparison.Ordinal);
    }
}
