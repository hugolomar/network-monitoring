using NetworkMonitoring.IntegrationConsole.IntegrationTests.Support;

namespace NetworkMonitoring.IntegrationConsole.IntegrationTests;

/// <summary>
/// Test suite for FakeDeviceReceiverIdempotency.
/// </summary>
public sealed class FakeDeviceReceiverIdempotencyTests
{
    /// <summary>
    /// Verifies that send duplicate events preserve single fake receiver effect.
    /// </summary>
    [Fact]
    public async Task Send_duplicate_events_preserve_single_fake_receiver_effect()
    {
        var receiver = new FakeDeviceReceiver();
        var client = FakeDeviceReceiverForwardingTests.CreateClient(receiver);

        await client.Send(FakeDeviceReceiverForwardingTests.Device(), CancellationToken.None);
        await client.Send(FakeDeviceReceiverForwardingTests.Device(), CancellationToken.None);

        Assert.Equal(2, receiver.Requests.Count);
        Assert.Equal(1, receiver.UniqueDeviceEffects);
        Assert.All(receiver.Requests, request => Assert.Equal("AA:BB:CC:DD:EE:FF", request.IdempotencyKey));
    }
}
