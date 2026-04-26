using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkMonitoring.Probe.Application.UseCases;

namespace NetworkMonitoring.Probe.Host.Services;

public sealed class ProbeWorker(
    ProcessObservationsUseCase useCase,
    ILogger<ProbeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Probe worker started.");
        await useCase.ExecuteAsync(stoppingToken);
    }
}
