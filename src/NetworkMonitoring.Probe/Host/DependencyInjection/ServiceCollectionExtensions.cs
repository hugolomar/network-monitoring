using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Probe.Application.Configuration;
using NetworkMonitoring.Probe.Application.Ports;
using NetworkMonitoring.Probe.Application.UseCases;
using NetworkMonitoring.Probe.Infrastructure.Publishing;
using NetworkMonitoring.Probe.Infrastructure.Traffic;

namespace NetworkMonitoring.Probe.Host.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProbeServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProbeOptions>(configuration.GetSection(ProbeOptions.SectionName));

        services.AddSingleton<TsharkObservationMapper>();
        services.AddSingleton<ConsoleRecordSerializer>();
        services.AddSingleton<ITrafficProvider, TsharkTrafficProvider>();
        services.AddSingleton<ConsolePublisher>();
        services.AddSingleton<IKafkaGenericRecordProducerFactory, KafkaGenericRecordProducerFactory>();
        services.AddSingleton<KafkaProbeEventPublisher>();
        services.AddSingleton<IMessagePublisher>(sp => CreateMessagePublisher(sp));
        services.AddSingleton<ProcessObservationsUseCase>();

        return services;
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
