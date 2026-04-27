using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

public sealed class RejectedEventTests
{
    [Fact]
    public async Task Process_acknowledges_malformed_event_without_forwarding()
    {
        var malformed = new ConsumedDeviceEvent("AA:BB:CC:DD:EE:FF", null, "devices.detected", 0, 1, "Unknown magic byte");
        var consumer = new FakeDeviceEventConsumer(malformed);
        var intake = new FakeDeviceIntakeClient();
        var useCase = new ProcessDeviceDetectionsUseCase(consumer, intake, NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        var outcome = await useCase.Process(malformed, CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.Rejected, outcome.Kind);
        Assert.Empty(intake.Sent);
        Assert.Equal([malformed], consumer.Acknowledged);
    }
}
