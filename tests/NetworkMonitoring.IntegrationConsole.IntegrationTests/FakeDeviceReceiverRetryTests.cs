using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.IntegrationTests.Support;

namespace NetworkMonitoring.IntegrationConsole.IntegrationTests;

public sealed class FakeDeviceReceiverRetryTests
{
    [Fact]
    public async Task Send_retries_transient_failure_before_success()
    {
        var receiver = new FakeDeviceReceiver();
        receiver.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        receiver.EnqueueResponse(HttpStatusCode.Accepted);
        var client = FakeDeviceReceiverForwardingTests.CreateClient(receiver);

        var outcome = await client.Send(FakeDeviceReceiverForwardingTests.Device(), CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.Succeeded, outcome.Kind);
        Assert.Equal(2, receiver.Requests.Count);
    }

    [Fact]
    public async Task Send_records_retry_exhaustion()
    {
        var receiver = new FakeDeviceReceiver();
        receiver.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        receiver.EnqueueResponse(HttpStatusCode.ServiceUnavailable);
        var client = FakeDeviceReceiverForwardingTests.CreateClient(receiver, attempts: 2);

        var outcome = await client.Send(FakeDeviceReceiverForwardingTests.Device(), CancellationToken.None);

        Assert.Equal(IngestionOutcomeKind.RetryExhausted, outcome.Kind);
        Assert.Equal(2, receiver.Requests.Count);
    }
}
