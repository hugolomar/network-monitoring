using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IMessagePublisher, ConsolePublisher>();
        services.AddSingleton<ProcessObservationsUseCase>();

        return services;
    }
}
