using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;

namespace NetworkMonitoring.Backend.Host.Services;

/// <summary>
/// Hosted service that periodically projects indexed sessions into the communication graph.
/// </summary>
public sealed class GraphProjectionHostedService(
    IServiceProvider serviceProvider,
    IHostEnvironment hostEnvironment,
    IOptions<BackendOptions> options,
    ILogger<GraphProjectionHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (hostEnvironment.IsEnvironment("Testing"))
        {
            return;
        }

        var projectionOptions = options.Value.Graph.ProjectionSource;
        if (!projectionOptions.Enabled)
        {
            logger.LogInformation("Graph projection sweep is disabled by configuration.");
            return;
        }

        var cadence = TimeSpan.FromMinutes(Math.Max(1, projectionOptions.SweepCadenceMinutes));

        await RunSweep(stoppingToken);

        using var timer = new PeriodicTimer(cadence);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunSweep(stoppingToken);
        }
    }

    private async Task RunSweep(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var source = scope.ServiceProvider.GetRequiredService<ISessionProjectionSource>();
            var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
            var windowEnd = DateTimeOffset.UtcNow;
            var records = await source.GetAggregatedSessions(DateTimeOffset.UnixEpoch, windowEnd, cancellationToken);
            await projector.ExecuteSweep(records, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutdown path
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Graph projection sweep failed.");
        }
    }
}
