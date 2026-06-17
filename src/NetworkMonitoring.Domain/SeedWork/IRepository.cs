namespace NetworkMonitoring.Domain.SeedWork;

/// <summary>
/// Base generic repository interface for Aggregate Roots.
/// </summary>
/// <typeparam name="T">The type of the aggregate root.</typeparam>
public interface IRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// Gets the Unit of Work associated with this repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}