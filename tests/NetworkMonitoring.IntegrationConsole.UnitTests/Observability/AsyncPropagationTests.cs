using System.Diagnostics;
using System.Reflection;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Ingestion;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Observability;

/// <summary>
/// Verifies async correlation propagation helpers.
/// </summary>
public sealed class AsyncPropagationTests
{
    /// <summary>
    /// Ensures correlation identifier is propagated into baggage context.
    /// </summary>
    [Fact]
    public void Apply_correlation_context_sets_baggage_key()
    {
        var method = typeof(KafkaDeviceEventConsumer).GetMethod(
            "ApplyCorrelationContext",
            BindingFlags.Static | BindingFlags.NonPublic);
        using var activity = new Activity("test-async-correlation");
        activity.Start();

        Assert.NotNull(method);
        method!.Invoke(null, [activity, "corr-async-1"]);

        Assert.Equal("corr-async-1", activity.Baggage.First(pair => pair.Key == "correlation.id").Value);
    }
}
