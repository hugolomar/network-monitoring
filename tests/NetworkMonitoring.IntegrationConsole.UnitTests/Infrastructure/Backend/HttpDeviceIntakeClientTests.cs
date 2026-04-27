using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Infrastructure.Backend;

public sealed class HttpDeviceIntakeClientTests
{
    [Fact]
    public async Task Send_posts_to_devices_with_json_body_and_idempotency_key()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.Accepted));
        var client = CreateClient(handler);

        var outcome = await client.Send(TestEvents.DeviceDetected(), CancellationToken.None);

        Assert.Equal("/devices", handler.Requests.Single().RequestUri!.PathAndQuery);
        Assert.Equal("AA:BB:CC:DD:EE:FF", handler.Requests.Single().Headers.GetValues("Idempotency-Key").Single());
        Assert.Contains("\"macAddress\":\"AA:BB:CC:DD:EE:FF\"", handler.Bodies.Single());
        Assert.Equal(NetworkMonitoring.IntegrationConsole.Application.Models.IngestionOutcomeKind.Succeeded, outcome.Kind);
    }

    private static HttpDeviceIntakeClient CreateClient(CapturingHandler handler) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
            new RetryOptions(3, TimeSpan.Zero),
            new DeviceIntakeRetryPolicy());

    private sealed class CapturingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return response;
        }
    }
}
