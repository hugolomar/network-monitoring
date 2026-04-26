using NetworkMonitoring.Domain.Entities;
using NetworkMonitoring.Probe.Application.Ports;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

public sealed class ConsolePublisher(ConsoleRecordSerializer serializer) : IMessagePublisher
{
    public Task PublishSessionDetected(Session session, CancellationToken cancellationToken)
    {
        Console.WriteLine(serializer.SerializeSession(session));
        return Task.CompletedTask;
    }

    public Task PublishDeviceDetected(Device device, CancellationToken cancellationToken)
    {
        Console.WriteLine(serializer.SerializeDevice(device));
        return Task.CompletedTask;
    }
}
