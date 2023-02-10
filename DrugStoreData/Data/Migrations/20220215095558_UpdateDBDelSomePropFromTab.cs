using Microsoft.EntityFrameworkCore.Migrations;

namespace DrugStore.Data.Migrations
{
    public partial class UpdateDBDelSomePropFromTab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrdersDetails");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "OrdersDetails");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "Drugs");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PharmacysOrders",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PharmacysOrders");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OrdersDetails",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<float>(
                name: "TotalPrice",
                table: "OrdersDetails",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "Drugs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
