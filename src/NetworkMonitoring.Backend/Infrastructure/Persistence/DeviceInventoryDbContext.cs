using Microsoft.EntityFrameworkCore;

namespace NetworkMonitoring.Backend.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the device inventory.
/// </summary>
/// <param name="options">The options to be used by the <see cref="DbContext"/>.</param>
public sealed class DeviceInventoryDbContext(DbContextOptions<DeviceInventoryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the set of device inventory records.
    /// </summary>
    public DbSet<DeviceInventoryRecord> Devices => Set<DeviceInventoryRecord>();

    /// <summary>
    /// Configures the model for the device inventory database.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var device = modelBuilder.Entity<DeviceInventoryRecord>();
        
        // Map to 'devices' table and configure primary key and unique constraints
        device.ToTable("devices");
        device.HasKey(record => record.Id);
        device.HasIndex(record => record.MacAddress).IsUnique();
        
        // Configure property constraints for MAC address, IP, and Hostname
        device.Property(record => record.MacAddress).IsRequired().HasMaxLength(17);
        device.Property(record => record.PrimaryIp).HasMaxLength(45);
        device.Property(record => record.Hostname).HasMaxLength(255);
        device.Property(record => record.ObservedIpsJson).IsRequired();
        device.Property(record => record.DiscoverySource).IsRequired().HasMaxLength(32);
    }
}
