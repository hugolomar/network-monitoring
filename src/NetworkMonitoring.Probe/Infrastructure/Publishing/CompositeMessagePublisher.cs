using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public sealed class CompositeMessagePublisher(IReadOnlyList<IMessagePublisher> publishers) : IMessagePublisher
{
    public async Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
    {
        foreach (var publisher in publishers)
        {
            await publisher.PublishSessionDetected(session, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        foreach (var publisher in publishers)
        {
            await publisher.PublishDeviceDetected(device, cancellationToken).ConfigureAwait(false);
        }
    }
}
