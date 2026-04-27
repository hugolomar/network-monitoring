using NetworkMonitoring.IntegrationConsole.Application.Models;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.UnitTests.Support;

internal sealed class FakeDeviceEventConsumer(params ConsumedDeviceEvent[] events) : IDeviceEventConsumer
{
    public List<ConsumedDeviceEvent> Acknowledged { get; } = [];

    public async IAsyncEnumerable<ConsumedDeviceEvent> Consume([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var consumedEvent in events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return consumedEvent;
            await Task.Yield();
        }
    }

    public Task Acknowledge(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken)
    {
        Acknowledged.Add(consumedEvent);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed class FakeDeviceIntakeClient(params IngestionOutcome[] outcomes) : IDeviceIntakeClient
{
    private readonly Queue<IngestionOutcome> _outcomes = new(outcomes);

    public List<DeviceDetectedEvent> Sent { get; } = [];

    public Task<IngestionOutcome> Send(DeviceDetectedEvent detectedEvent, CancellationToken cancellationToken)
    {
        Sent.Add(detectedEvent);
        return Task.FromResult(_outcomes.Count > 0 ? _outcomes.Dequeue() : IngestionOutcome.Succeeded());
    }
}
