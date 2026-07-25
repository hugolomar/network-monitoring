using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Observability;

/// <summary>
/// Verifies ingestion throughput metric emission (FR-015).
/// </summary>
public sealed class IngestionThroughputMetricTests
{
    /// <summary>
    /// Ensures a successful forward increments the succeeded outcome counter.
    /// </summary>
    [Fact]
    public async Task Process_emits_succeeded_ingestion_throughput()
    {
        var telemetry = new RecordingIngestionFlowTelemetry();
        var consumed = TestEvents.Consumed();
        var useCase = new ProcessDeviceDetectionsUseCase(
            new FakeDeviceEventConsumer(consumed),
            new FakeDeviceIntakeClient(IngestionOutcome.Succeeded()),
            telemetry,
            NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        var outcome = await useCase.Process(consumed, CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.Succeeded, outcome.Kind);
        Assert.Equal(["succeeded"], telemetry.Outcomes);
    }

    /// <summary>
    /// Ensures a malformed event increments the rejected outcome counter.
    /// </summary>
    [Fact]
    public async Task Process_emits_rejected_ingestion_throughput_for_malformed_event()
    {
        var telemetry = new RecordingIngestionFlowTelemetry();
        var consumed = new ConsumedDeviceEvent(null, null, "devices.detected", 0, 1, "bad payload");
        var useCase = new ProcessDeviceDetectionsUseCase(
            new FakeDeviceEventConsumer(consumed),
            new FakeDeviceIntakeClient(IngestionOutcome.Succeeded()),
            telemetry,
            NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        var outcome = await useCase.Process(consumed, CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.Rejected, outcome.Kind);
        Assert.Equal(["rejected"], telemetry.Outcomes);
    }

    private sealed class RecordingIngestionFlowTelemetry : IIngestionFlowTelemetry
    {
        public List<string> Outcomes { get; } = [];

        public void TrackIngestion(string outcome) => Outcomes.Add(outcome);

        public void TrackConsumerLag(string topic, int partition, long lag) { }
    }
}
