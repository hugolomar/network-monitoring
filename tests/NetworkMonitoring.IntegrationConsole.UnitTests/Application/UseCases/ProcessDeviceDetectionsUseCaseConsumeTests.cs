using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

/// <summary>
/// Test suite for ProcessDeviceDetectionsUseCaseConsume.
/// </summary>
public sealed class ProcessDeviceDetectionsUseCaseConsumeTests
{
    /// <summary>
    /// Verifies that run forwards valid event and acknowledges processing position.
    /// </summary>
    [Fact]
    public async Task Run_forwards_valid_event_and_acknowledges_processing_position()
    {
        var consumed = TestEvents.Consumed();
        var consumer = new FakeDeviceEventConsumer(consumed);
        var intake = new FakeDeviceIntakeClient(IngestionOutcome.Succeeded());
        var useCase = new ProcessDeviceDetectionsUseCase(consumer, intake, NoOpIngestionFlowTelemetry.Instance, NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        await useCase.Run(CancellationToken.None);

        Assert.Single(intake.Sent);
        Assert.Equal([consumed], consumer.Acknowledged);
    }
}
