using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarInTime.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDecimalForCoordinatesToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Ports",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Ports",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,6)",
                oldPrecision: 8,
                oldScale: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Ports",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Ports",
                type: "numeric(8,6)",
                precision: 8,
                scale: 6,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
