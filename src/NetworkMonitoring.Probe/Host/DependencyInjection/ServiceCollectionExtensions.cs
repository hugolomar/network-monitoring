using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Publishing;
using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.Host.DependencyInjection;
/// <summary>
/// Provides extension methods for registering probe-specific services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application and infrastructure services required for the probe to operate.
    /// Includes configuration binding, traffic providers, and messaging publishers.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddProbeServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ProbeOptions>()
            .Bind(configuration.GetSection(ProbeOptions.SectionName))
            .Validate(
                options => Enum.IsDefined(options.InputMode),
                "Probe:InputMode must be one of: Live, DeterministicTest.")
            .Validate(
                options =>
                    options.InputMode != CaptureInputMode.DeterministicTest
                    || !string.IsNullOrWhiteSpace(options.DeterministicTestPcapPath),
                "Probe:DeterministicTestPcapPath is required when Probe:InputMode is DeterministicTest.")
            .Validate(
                options =>
                    options.InputMode != CaptureInputMode.DeterministicTest
                    || File.Exists(options.DeterministicTestPcapPath),
                "Probe:DeterministicTestPcapPath must point to an existing file when Probe:InputMode is DeterministicTest.")
            .Validate(
                options => !options.EnableKafka || !string.IsNullOrWhiteSpace(options.KafkaBootstrapServers),
                "Probe:KafkaBootstrapServers is required when Probe:EnableKafka is true.")
            .Validate(
                options => !options.EnableKafka || !string.IsNullOrWhiteSpace(options.SchemaRegistryUrl),
                "Probe:SchemaRegistryUrl is required when Probe:EnableKafka is true.")
            .Validate(
                options => options.DeterministicPlaybackSpeed >= 0,
                "Probe:DeterministicPlaybackSpeed must be zero or greater.")
            .ValidateOnStart();

        services.AddSingleton<TsharkObservationMapper>();
        services.AddSingleton<ConsoleRecordSerializer>();
        services.AddSingleton<TsharkTrafficProvider>();
        services.AddSingleton<PcapFileTrafficProvider>();
        services.AddSingleton<ITrafficProvider>(sp => CreateTrafficProvider(sp));
        services.AddSingleton<ConsolePublisher>();
        services.AddSingleton<IKafkaGenericRecordProducerFactory, KafkaGenericRecordProducerFactory>();
        services.AddSingleton<KafkaProbeEventPublisher>();
        services.AddSingleton<IMessagePublisher>(sp => CreateMessagePublisher(sp));
        services.AddSingleton<ProcessObservationsUseCase>();

        return services;
    }

    private static ITrafficProvider CreateTrafficProvider(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<ProbeOptions>>().Value;
        return options.InputMode switch
        {
            CaptureInputMode.DeterministicTest => sp.GetRequiredService<PcapFileTrafficProvider>(),
            _ => sp.GetRequiredService<TsharkTrafficProvider>()
        };
    }

    private static IMessagePublisher CreateMessagePublisher(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<ProbeOptions>>().Value;
        var publishers = new List<IMessagePublisher>();
        if (options.EnableConsole)
        {
            publishers.Add(sp.GetRequiredService<ConsolePublisher>());
        }

        if (options.EnableKafka)
        {
            publishers.Add(sp.GetRequiredService<KafkaProbeEventPublisher>());
        }

        if (publishers.Count == 0)
        {
            publishers.Add(sp.GetRequiredService<ConsolePublisher>());
        }

        return publishers.Count == 1 ? publishers[0] : new CompositeMessagePublisher(publishers);
    }
}
