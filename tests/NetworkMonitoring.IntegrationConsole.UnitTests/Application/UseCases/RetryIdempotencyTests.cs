using Microsoft.Extensions.Logging.Abstractions;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Application.UseCases;

public sealed class RetryIdempotencyTests
{
    [Fact]
    public async Task Process_preserves_normalized_mac_for_forwarding_identity()
    {
        var consumed = TestEvents.Consumed(TestEvents.DeviceDetected("aa-bb-cc-dd-ee-ff"), "AABBCCDDEEFF");
        var consumer = new FakeDeviceEventConsumer(consumed);
        var intake = new FakeDeviceIntakeClient(IngestionOutcome.RetryExhausted(3, 503, "retry exhausted"));
        var useCase = new ProcessDeviceDetectionsUseCase(consumer, intake, NullLogger<ProcessDeviceDetectionsUseCase>.Instance);

        await useCase.Process(consumed, CancellationToken.None);

        Assert.Equal("AA:BB:CC:DD:EE:FF", intake.Sent.Single().MacAddress);
    }
}
