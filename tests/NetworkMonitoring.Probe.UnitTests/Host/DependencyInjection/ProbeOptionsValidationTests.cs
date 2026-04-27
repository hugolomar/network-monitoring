using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Host.DependencyInjection;

namespace NetworkMonitoring.Probe.UnitTests.Host.DependencyInjection;

public sealed class ProbeOptionsValidationTests
{
    [Fact]
    public void ProbeOptions_WhenKafkaEnabled_RequireKafkaBootstrapServers()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:EnableKafka"] = "true",
            ["Probe:SchemaRegistryUrl"] = "http://localhost:8081",
        });

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ProbeOptions>>().Value);

        Assert.Contains("Probe:KafkaBootstrapServers is required", exception.Message);
    }

    [Fact]
    public void ProbeOptions_WhenKafkaEnabled_RequireSchemaRegistryUrl()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:EnableKafka"] = "true",
            ["Probe:KafkaBootstrapServers"] = "localhost:9092",
        });

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ProbeOptions>>().Value);

        Assert.Contains("Probe:SchemaRegistryUrl is required", exception.Message);
    }

    [Fact]
    public void ProbeOptions_WhenKafkaDisabled_DoNotRequireKafkaConnectionSettings()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:EnableKafka"] = "false",
        });

        var options = provider.GetRequiredService<IOptions<ProbeOptions>>().Value;

        Assert.False(options.EnableKafka);
        Assert.Null(options.KafkaBootstrapServers);
        Assert.Null(options.SchemaRegistryUrl);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProbeServices(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
