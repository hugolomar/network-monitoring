using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Application.Ports;

public interface IDeviceEventConsumer : IAsyncDisposable
{
    IAsyncEnumerable<ConsumedDeviceEvent> Consume(CancellationToken cancellationToken);

    Task Acknowledge(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken);
}
