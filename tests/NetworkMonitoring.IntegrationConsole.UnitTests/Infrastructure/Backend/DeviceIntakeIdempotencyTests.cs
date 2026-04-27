using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Infrastructure.Backend;

public sealed class DeviceIntakeIdempotencyTests
{
    [Fact]
    public async Task Send_uses_normalized_mac_as_idempotency_key()
    {
        var handler = new CapturingHandler();
        var client = new HttpDeviceIntakeClient(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
            new RetryOptions(1, TimeSpan.Zero),
            new DeviceIntakeRetryPolicy());

        await client.Send(TestEvents.DeviceDetected("AA:BB:CC:DD:EE:FF"), CancellationToken.None);

        Assert.Equal("AA:BB:CC:DD:EE:FF", handler.IdempotencyKeys.Single());
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public List<string> IdempotencyKeys { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IdempotencyKeys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        }
    }
}
