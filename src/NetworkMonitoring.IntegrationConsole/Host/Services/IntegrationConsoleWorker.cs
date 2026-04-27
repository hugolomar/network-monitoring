using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;

namespace NetworkMonitoring.IntegrationConsole.Host.Services;

public sealed class IntegrationConsoleWorker(
    ProcessDeviceDetectionsUseCase useCase,
    ILogger<IntegrationConsoleWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Integration Console device ingestion worker starting");
        await useCase.Run(stoppingToken);
    }
}
