namespace NetworkMonitoring.Backend.Application.Ports;

public interface IInventoryUnitOfWork
{
    Task SaveChanges(CancellationToken cancellationToken);
}
