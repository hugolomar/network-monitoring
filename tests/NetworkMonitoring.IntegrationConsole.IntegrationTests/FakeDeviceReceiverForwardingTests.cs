using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.IntegrationTests.Support;

namespace NetworkMonitoring.IntegrationConsole.IntegrationTests;

public sealed class FakeDeviceReceiverForwardingTests
{
    [Fact]
    public async Task Send_forwards_valid_device_to_fake_receiver()
    {
        var receiver = new FakeDeviceReceiver();
        var client = CreateClient(receiver);

        var outcome = await client.Send(Device(), CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.Succeeded, outcome.Kind);
        Assert.Equal("/devices", receiver.Requests.Single().Path);
        Assert.Equal("AA:BB:CC:DD:EE:FF", receiver.Requests.Single().IdempotencyKey);
    }

    internal static HttpDeviceIntakeClient CreateClient(FakeDeviceReceiver receiver, int attempts = 3) =>
        new(
            new HttpClient(receiver.CreateHandler()) { BaseAddress = new Uri("http://fake") },
            new RetryOptions(attempts, TimeSpan.Zero),
            new DeviceIntakeRetryPolicy());

    internal static DeviceDetectedEvent Device(string macAddress = "AA:BB:CC:DD:EE:FF") =>
        new(
            "DeviceDetected",
            DateTimeOffset.Parse("2026-04-27T12:05:01.0000000+00:00"),
            "probe",
            1,
            null,
            macAddress,
            "192.168.1.10",
            "switch-01",
            ["192.168.1.10"],
            DateTimeOffset.Parse("2026-04-27T12:00:00.0000000+00:00"),
            DateTimeOffset.Parse("2026-04-27T12:05:00.0000000+00:00"),
            "TRAFFIC");
}
