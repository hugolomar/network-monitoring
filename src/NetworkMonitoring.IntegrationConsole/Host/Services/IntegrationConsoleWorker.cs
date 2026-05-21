using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;

namespace NetworkMonitoring.IntegrationConsole.Host.Services;

/// <summary>
/// Background worker that orchestrates the device ingestion process.
/// </summary>
/// <param name="useCase">The device processing use case.</param>
/// <param name="logger">The logger.</param>
public sealed class IntegrationConsoleWorker(
    ProcessDeviceDetectionsUseCase useCase,
    ILogger<IntegrationConsoleWorker> logger) : BackgroundService
{
    /// <summary>
    /// Executes the background processing loop.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the host is shutting down.</param>
    /// <returns>A task representing the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Integration Console device ingestion worker starting");
        await useCase.Run(stoppingToken);
    }
}
