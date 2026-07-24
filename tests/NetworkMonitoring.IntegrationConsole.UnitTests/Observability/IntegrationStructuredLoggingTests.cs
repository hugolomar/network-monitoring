using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.Host.DependencyInjection;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Observability;

/// <summary>
/// Verifies integration-console observability service registrations.
/// </summary>
public sealed class IntegrationStructuredLoggingTests
{
    /// <summary>
    /// Ensures integration-console services include processing use case and health checks.
    /// </summary>
    [Fact]
    public async Task Integration_console_services_register_observability_baseline_dependencies()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IntegrationConsole:KafkaDeviceTopic"] = "devices.detected",
                ["IntegrationConsole:BackendBaseUrl"] = "http://localhost:5090",
                ["IntegrationConsole:KafkaBootstrapServers"] = "localhost:9092",
                ["IntegrationConsole:SchemaRegistryUrl"] = "http://localhost:8081"
            })
            .Build();

        services.AddIntegrationConsole(configuration);
        await using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ProcessDeviceDetectionsUseCase>());
        Assert.NotNull(provider.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>());
    }
}
