using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Host.DependencyInjection;
using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.UnitTests.Host.DependencyInjection;

/// <summary>
/// Test suite for ProbeOptionsValidation.
/// </summary>
public sealed class ProbeOptionsValidationTests
{
    /// <summary>
    /// Verifies that probe options when kafka enabled require kafka bootstrap servers.
    /// </summary>
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

    /// <summary>
    /// Verifies that probe options when kafka enabled require schema registry url.
    /// </summary>
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

    /// <summary>
    /// Verifies that probe options when kafka disabled do not require kafka connection settings.
    /// </summary>
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

    /// <summary>
    /// Verifies deterministic test mode requires deterministic pcap path.
    /// </summary>
    [Fact]
    public void ProbeOptions_WhenDeterministicModeEnabled_RequirePcapPath()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:InputMode"] = "DeterministicTest",
        });

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ProbeOptions>>().Value);

        Assert.Contains("Probe:DeterministicTestPcapPath is required", exception.Message);
    }

    /// <summary>
    /// Verifies deterministic test mode requires existing pcap file.
    /// </summary>
    [Fact]
    public void ProbeOptions_WhenDeterministicModeEnabled_RequireExistingPcapFile()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:InputMode"] = "DeterministicTest",
            ["Probe:DeterministicTestPcapPath"] = "does-not-exist.pcap",
        });

        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ProbeOptions>>().Value);

        Assert.Contains("Probe:DeterministicTestPcapPath must point to an existing file", exception.Message);
    }

    /// <summary>
    /// Verifies live mode resolves tshark provider.
    /// </summary>
    [Fact]
    public void ProbeOptions_WhenLiveModeConfigured_ResolveTsharkTrafficProvider()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Probe:InputMode"] = "Live",
        });

        var trafficProvider = provider.GetRequiredService<ITrafficProvider>();

        Assert.IsType<TsharkTrafficProvider>(trafficProvider);
    }

    /// <summary>
    /// Verifies deterministic mode resolves pcap provider.
    /// </summary>
    [Fact]
    public void ProbeOptions_WhenDeterministicModeConfigured_ResolvePcapFileTrafficProvider()
    {
        var pcapPath = CreateTempPcapFile();
        try
        {
            using var provider = BuildProvider(new Dictionary<string, string?>
            {
                ["Probe:InputMode"] = "DeterministicTest",
                ["Probe:DeterministicTestPcapPath"] = pcapPath,
            });

            var trafficProvider = provider.GetRequiredService<ITrafficProvider>();

            Assert.IsType<PcapFileTrafficProvider>(trafficProvider);
        }
        finally
        {
            File.Delete(pcapPath);
        }
    }

    /// <summary>
    /// Verifies negative deterministic playback speed fails validation.
    /// </summary>
    [Fact]
    public void ProbeOptions_WhenDeterministicPlaybackSpeedNegative_FailsValidation()
    {
        var pcapPath = CreateTempPcapFile();
        try
        {
            using var provider = BuildProvider(new Dictionary<string, string?>
            {
                ["Probe:InputMode"] = "DeterministicTest",
                ["Probe:DeterministicTestPcapPath"] = pcapPath,
                ["Probe:DeterministicPlaybackSpeed"] = "-1",
            });

            var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ProbeOptions>>().Value);

            Assert.Contains("Probe:DeterministicPlaybackSpeed must be zero or greater", exception.Message);
        }
        finally
        {
            File.Delete(pcapPath);
        }
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

    private static string CreateTempPcapFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"probe-deterministic-{Guid.NewGuid():N}.pcap");
        File.WriteAllBytes(tempPath, []);
        return tempPath;
    }
}
