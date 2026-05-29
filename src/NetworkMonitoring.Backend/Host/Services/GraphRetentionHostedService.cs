using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.UseCases;

namespace NetworkMonitoring.Backend.Host.Services;

/// <summary>
/// Hosted service that executes graph retention on the configured 24-hour cadence.
/// </summary>
public sealed class GraphRetentionHostedService(
    IServiceProvider serviceProvider,
    IOptions<BackendOptions> options,
    ILogger<GraphRetentionHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cadence = TimeSpan.FromHours(options.Value.Graph.RetentionCadenceHours);
        if (cadence <= TimeSpan.Zero)
        {
            cadence = TimeSpan.FromHours(24);
        }

        using var timer = new PeriodicTimer(cadence);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<RunGraphRetentionSweepUseCase>();
                await useCase.Execute(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown path
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Graph retention sweep failed.");
            }
        }
    }
}
