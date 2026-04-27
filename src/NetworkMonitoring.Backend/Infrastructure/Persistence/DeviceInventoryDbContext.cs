using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

public sealed class DeviceInventoryDbContext(DbContextOptions<DeviceInventoryDbContext> options) : DbContext(options)
{
    public DbSet<DeviceInventoryRecord> Devices => Set<DeviceInventoryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var device = modelBuilder.Entity<DeviceInventoryRecord>();
        device.ToTable("devices");
        device.HasKey(record => record.Id);
        device.HasIndex(record => record.MacAddress).IsUnique();
        device.Property(record => record.MacAddress).IsRequired().HasMaxLength(17);
        device.Property(record => record.PrimaryIp).HasMaxLength(45);
        device.Property(record => record.Hostname).HasMaxLength(255);
        device.Property(record => record.ObservedIpsJson).IsRequired();
        device.Property(record => record.DiscoverySource).IsRequired().HasMaxLength(32);
    }
}
