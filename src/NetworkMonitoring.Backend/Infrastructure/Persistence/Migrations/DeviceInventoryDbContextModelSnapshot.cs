using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetworkMonitoring.Backend.Infrastructure.Persistence;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NetworkMonitoring.Backend.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DeviceInventoryDbContext))]
public partial class DeviceInventoryDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.7")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("NetworkMonitoring.Backend.Infrastructure.Persistence.DeviceInventoryRecord", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<DateTimeOffset>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("DiscoverySource")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<DateTimeOffset>("FirstSeenUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Hostname")
                .HasMaxLength(255)
                .HasColumnType("character varying(255)");

            b.Property<DateTimeOffset>("LastSeenUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("MacAddress")
                .IsRequired()
                .HasMaxLength(17)
                .HasColumnType("character varying(17)");

            b.Property<string>("ObservedIpsJson")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("PrimaryIp")
                .HasMaxLength(45)
                .HasColumnType("character varying(45)");

            b.Property<DateTimeOffset>("UpdatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("MacAddress")
                .IsUnique();

            b.ToTable("devices");
        });
    }
}
