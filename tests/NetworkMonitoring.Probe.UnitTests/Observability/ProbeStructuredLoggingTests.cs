using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Host.DependencyInjection;

namespace NetworkMonitoring.Probe.UnitTests.Observability;

/// <summary>
/// Verifies probe observability service registrations.
/// </summary>
public sealed class ProbeStructuredLoggingTests
{
    /// <summary>
    /// Ensures probe services include processing use case and health checks.
    /// </summary>
    [Fact]
    public void Probe_services_register_observability_baseline_dependencies()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Probe:InputMode"] = "Live",
                ["Probe:EnableConsole"] = "true",
                ["Probe:EnableKafka"] = "false",
                ["Probe:TSharkPath"] = "tshark",
                ["Probe:InterfaceName"] = "eth0"
            })
            .Build();

        services.AddProbeServices(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ProcessObservationsUseCase>());
        Assert.NotNull(provider.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>());
    }
}
