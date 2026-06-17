namespace NetworkMonitoring.Backend.Application.Ports;

/// <summary>
/// Defines the contract for a unit of work managing inventory persistence.
/// </summary>
public interface IInventoryUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the persistent store.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveChanges(CancellationToken cancellationToken);
}
