using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace NetworkMonitoring.IntegrationConsole.IntegrationTests.Support;

internal sealed class FakeDeviceReceiver
{
    private readonly ConcurrentDictionary<string, JsonDocument> _devicesByIdempotencyKey = new(StringComparer.Ordinal);
    private readonly Queue<HttpStatusCode> _responses = new();

    public List<FakeDeviceRequest> Requests { get; } = [];

    public int UniqueDeviceEffects => _devicesByIdempotencyKey.Count;

    public void EnqueueResponse(HttpStatusCode statusCode) => _responses.Enqueue(statusCode);

    public HttpMessageHandler CreateHandler() => new Handler(this);

    private HttpResponseMessage Handle(HttpRequestMessage request, string body)
    {
        var idempotencyKey = request.Headers.TryGetValues("Idempotency-Key", out var values)
            ? values.Single()
            : string.Empty;

        Requests.Add(new FakeDeviceRequest(request.RequestUri!.PathAndQuery, idempotencyKey, body));

        if (!_responses.TryDequeue(out var statusCode))
        {
            statusCode = HttpStatusCode.Accepted;
        }

        if ((int)statusCode >= 200 && (int)statusCode <= 299)
        {
            _devicesByIdempotencyKey.TryAdd(idempotencyKey, JsonDocument.Parse(body));
        }

        return new HttpResponseMessage(statusCode);
    }

    private sealed class Handler(FakeDeviceReceiver receiver) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return receiver.Handle(request, body);
        }
    }
}

internal sealed record FakeDeviceRequest(string Path, string IdempotencyKey, string Body);
