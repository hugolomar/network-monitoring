using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Application.UseCases;

/// <summary>
/// Executes one graph retention sweep.
/// </summary>
public sealed class RunGraphRetentionSweepUseCase(
    IGraphRetentionRepository retentionRepository,
    IGraphTelemetry telemetry,
    IClock clock,
    IOptions<BackendOptions> options)
{
    /// <summary>
    /// Runs stale-edge and orphan cleanup and emits retention diagnostics.
    /// </summary>
    public async Task<GraphRetentionSweepOutcome> Execute(CancellationToken cancellationToken)
    {
        var cutoffUtc = clock.UtcNow.AddDays(-options.Value.Graph.RetentionWindowDays);
        var outcome = await retentionRepository.Sweep(cutoffUtc, cancellationToken);
        telemetry.TrackRetention(outcome);
        return outcome;
    }
}
