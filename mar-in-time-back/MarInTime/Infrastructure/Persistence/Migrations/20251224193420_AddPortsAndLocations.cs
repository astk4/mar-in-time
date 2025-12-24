using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarInTime.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPortsAndLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    IsoId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.IsoId);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CountryId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => new { x.Id, x.CountryId });
                    table.ForeignKey(
                        name: "FK_Ports_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "IsoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortRoute",
                columns: table => new
                {
                    RoutesId = table.Column<byte>(type: "smallint", nullable: false),
                    PortsId = table.Column<string>(type: "text", nullable: false),
                    PortsCountryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortRoute", x => new { x.RoutesId, x.PortsId, x.PortsCountryId });
                    table.ForeignKey(
                        name: "FK_PortRoute_Ports_PortsId_PortsCountryId",
                        columns: x => new { x.PortsId, x.PortsCountryId },
                        principalTable: "Ports",
                        principalColumns: new[] { "Id", "CountryId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortRoute_Routes_RoutesId",
                        column: x => x.RoutesId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortRoute_PortsId_PortsCountryId",
                table: "PortRoute",
                columns: new[] { "PortsId", "PortsCountryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ports_CountryId",
                table: "Ports",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortRoute");

            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
