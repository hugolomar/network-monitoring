using NetworkMonitoring.IntegrationConsole.Application.Models;

namespace NetworkMonitoring.IntegrationConsole.Application.Ports;

/// <summary>
/// Port for consuming device events from a message broker.
/// </summary>
public interface IDeviceEventConsumer : IAsyncDisposable
{
    /// <summary>
    /// Starts consuming events and returns an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous stream of <see cref="ConsumedDeviceEvent"/>.</returns>
    IAsyncEnumerable<ConsumedDeviceEvent> Consume(CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges that an event has been processed.
    /// </summary>
    /// <param name="consumedEvent">The event to acknowledge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Acknowledge(ConsumedDeviceEvent consumedEvent, CancellationToken cancellationToken);
}
