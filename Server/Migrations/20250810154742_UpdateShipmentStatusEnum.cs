using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSigned",
                table: "ShipmentDocuments");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ShipmentDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ShipmentDocuments");

            migrationBuilder.AddColumn<bool>(
                name: "IsSigned",
                table: "ShipmentDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
