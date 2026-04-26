using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Application.Ports;

public interface IMessagePublisher
{
    Task PublishSessionDetected(Session session, CancellationToken cancellationToken);

    Task PublishDeviceDetected(Device device, CancellationToken cancellationToken);
}
