using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkMonitoring.Probe.Application.UseCases;

namespace NetworkMonitoring.Probe.Host.Services;

/// <summary>
/// A background worker that runs the probe processing loop as a long-running service.
/// </summary>
public sealed class ProbeWorker(
    ProcessObservationsUseCase useCase,
    ILogger<ProbeWorker> logger) : BackgroundService
{
    /// <summary>
    /// Executes the main probe logic in the background.
    /// </summary>
    /// <param name="stoppingToken">A token to observe for service shutdown.</param>
    /// <returns>A task representing the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Probe worker started.");
        await useCase.ExecuteAsync(stoppingToken);
    }
}
