using Microsoft.EntityFrameworkCore;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

/// <summary>
/// Entity Framework implementation of the device inventory repository.
/// </summary>
/// <param name="dbContext">The database context for device inventory.</param>
/// <param name="clock">The system clock provider.</param>
public sealed class EfDeviceInventoryRepository(DeviceInventoryDbContext dbContext, IClock clock)
    : IDeviceInventoryRepository, IInventoryUnitOfWork
{
    /// <inheritdoc />
    public async Task<Device?> GetByMacAddress(string normalizedMacAddress, CancellationToken cancellationToken)
    {
        var record = await dbContext.Devices
            .SingleOrDefaultAsync(device => device.MacAddress == normalizedMacAddress, cancellationToken);

        return record is null ? null : DeviceInventoryMapper.ToDomain(record);
    }

    /// <inheritdoc />
    public async Task Add(Device device, CancellationToken cancellationToken)
    {
        await dbContext.Devices.AddAsync(DeviceInventoryMapper.ToRecord(device, clock.UtcNow), cancellationToken);
    }

    /// <inheritdoc />
    public async Task Update(Device device, CancellationToken cancellationToken)
    {
        var record = await dbContext.Devices.SingleAsync(
            existing => existing.MacAddress == device.MacAddress.Value,
            cancellationToken);

        // Map domain changes to the tracked entity record
        DeviceInventoryMapper.UpdateRecord(record, device, clock.UtcNow);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Device>> List(CancellationToken cancellationToken)
    {
        var records = await dbContext.Devices
            .AsNoTracking()
            .OrderBy(device => device.MacAddress)
            .ToArrayAsync(cancellationToken);

        return records.Select(DeviceInventoryMapper.ToDomain).ToArray();
    }

    /// <inheritdoc />
    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
