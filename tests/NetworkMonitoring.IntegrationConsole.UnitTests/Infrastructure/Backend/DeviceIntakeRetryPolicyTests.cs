using System.Net;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.UnitTests.Support;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Infrastructure.Backend;

public sealed class DeviceIntakeRetryPolicyTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData((HttpStatusCode)429)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Send_retries_transient_status_codes(HttpStatusCode statusCode)
    {
        var handler = new SequencedHandler(
            new HttpResponseMessage(statusCode),
            new HttpResponseMessage(HttpStatusCode.Accepted));
        var client = CreateClient(handler);

        var outcome = await client.Send(TestEvents.DeviceDetected(), CancellationToken.None);

        Assert.Equal(2, handler.Attempts);
        Assert.Equal(IngestionOutcomeKind.Succeeded, outcome.Kind);
    }

    [Fact]
    public async Task Send_retries_network_failures_until_success()
    {
        var handler = new SequencedHandler(
            new HttpRequestException("connection refused"),
            new HttpResponseMessage(HttpStatusCode.Accepted));
        var client = CreateClient(handler);

        var outcome = await client.Send(TestEvents.DeviceDetected(), CancellationToken.None);

        Assert.Equal(2, handler.Attempts);
        Assert.Equal(IngestionOutcomeKind.Succeeded, outcome.Kind);
    }

    private static HttpDeviceIntakeClient CreateClient(SequencedHandler handler) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
            new RetryOptions(3, TimeSpan.Zero),
            new DeviceIntakeRetryPolicy());

    private sealed class SequencedHandler(params object[] responses) : HttpMessageHandler
    {
        private readonly Queue<object> _responses = new(responses);

        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Attempts++;
            var response = _responses.Dequeue();
            if (response is Exception exception)
            {
                throw exception;
            }

            return Task.FromResult((HttpResponseMessage)response);
        }
    }
}
