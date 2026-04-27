using Microsoft.EntityFrameworkCore;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

public sealed class EfDeviceInventoryRepository(DeviceInventoryDbContext dbContext, IClock clock)
    : IDeviceInventoryRepository, IInventoryUnitOfWork
{
    public async Task<Device?> GetByMacAddress(string normalizedMacAddress, CancellationToken cancellationToken)
    {
        var record = await dbContext.Devices
            .SingleOrDefaultAsync(device => device.MacAddress == normalizedMacAddress, cancellationToken);

        return record is null ? null : DeviceInventoryMapper.ToDomain(record);
    }

    public async Task Add(Device device, CancellationToken cancellationToken)
    {
        await dbContext.Devices.AddAsync(DeviceInventoryMapper.ToRecord(device, clock.UtcNow), cancellationToken);
    }

    public async Task Update(Device device, CancellationToken cancellationToken)
    {
        var record = await dbContext.Devices.SingleAsync(
            existing => existing.MacAddress == device.MacAddress.Value,
            cancellationToken);

        DeviceInventoryMapper.UpdateRecord(record, device, clock.UtcNow);
    }

    public async Task<IReadOnlyCollection<Device>> List(CancellationToken cancellationToken)
    {
        var records = await dbContext.Devices
            .AsNoTracking()
            .OrderBy(device => device.MacAddress)
            .ToArrayAsync(cancellationToken);

        return records.Select(DeviceInventoryMapper.ToDomain).ToArray();
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
