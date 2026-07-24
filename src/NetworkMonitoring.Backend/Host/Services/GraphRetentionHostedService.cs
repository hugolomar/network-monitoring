using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Services;
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
    private static readonly ActivitySource ActivitySource = new("NetworkMonitoring.Backend.Alerting");
    private static readonly Meter AlertMeter = new("NetworkMonitoring.Backend.Alerting");
    private static readonly Counter<long> AlertCounter = AlertMeter.CreateCounter<long>("critical_flow_alerts_total");

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
                var evaluator = scope.ServiceProvider.GetRequiredService<CriticalFlowObjectiveEvaluator>();
                var outcome = await useCase.Execute(stoppingToken);

                using var activity = ActivitySource.StartActivity("critical-flow.evaluate", ActivityKind.Internal);
                var alert = evaluator.EvaluateRetentionDegradation(
                    outcome,
                    correlationId: activity?.TraceId.ToString(),
                    traceId: activity?.TraceId.ToString());
                if (alert is not null)
                {
                    AlertCounter.Add(1, new KeyValuePair<string, object?>("flowId", alert.FlowId));
                    logger.LogWarning(
                        "Critical flow objective breach detected. flowId={FlowId} reason={BreachReason} impactedComponent={ImpactedComponent} severity={Severity} correlationId={CorrelationId} traceId={TraceId}",
                        alert.FlowId,
                        alert.BreachReason,
                        alert.ImpactedComponent,
                        alert.Severity,
                        alert.CorrelationId ?? string.Empty,
                        alert.TraceId ?? string.Empty);
                }
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
