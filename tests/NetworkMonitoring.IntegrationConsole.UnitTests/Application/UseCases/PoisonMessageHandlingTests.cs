using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

public sealed class PoisonMessageHandlingTests
{
    [Fact]
    public async Task Run_continues_after_poison_event()
    {
        var poison = new ConsumedDeviceEvent(null, null, "devices.detected", 0, 1, "poison");
        var valid = TestEvents.Consumed(key: "AA:BB:CC:DD:EE:FF");
        var consumer = new FakeDeviceEventConsumer(poison, valid);
        var intake = new FakeDeviceIntakeClient(IngestionOutcome.Succeeded());
        var useCase = new ProcessDeviceDetectionsUseCase(consumer, intake, NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        await useCase.Run(CancellationToken.None);

        Assert.Single(intake.Sent);
        Assert.Equal([poison, valid], consumer.Acknowledged);
    }
}
