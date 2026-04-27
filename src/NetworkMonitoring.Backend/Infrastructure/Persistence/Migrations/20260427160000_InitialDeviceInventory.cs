using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NetworkMonitoring.Backend.Infrastructure.Persistence.Migrations;

public partial class InitialDeviceInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "devices",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                MacAddress = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                PrimaryIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                Hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                ObservedIpsJson = table.Column<string>(type: "text", nullable: false),
                FirstSeenUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastSeenUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                DiscoverySource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_devices", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_devices_MacAddress",
            table: "devices",
            column: "MacAddress",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "devices");
    }
}
