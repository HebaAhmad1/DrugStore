using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DrugStore.Data.Migrations
{
    public partial class EditTabDrug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdersDetails_PharmacysOrders_PharmacyOrderId",
                table: "OrdersDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "UpdateAt",
                table: "Drugs");

            migrationBuilder.RenameColumn(
                name: "PharmacyOrderId",
                table: "OrdersDetails",
                newName: "PharmacyOrdersId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdersDetails_PharmacyOrderId",
                table: "OrdersDetails",
                newName: "IX_OrdersDetails_PharmacyOrdersId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OrdersDetails",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdersDetails_PharmacysOrders_PharmacyOrdersId",
                table: "OrdersDetails",
                column: "PharmacyOrdersId",
                principalTable: "PharmacysOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdersDetails_PharmacysOrders_PharmacyOrdersId",
                table: "OrdersDetails");

            migrationBuilder.RenameColumn(
                name: "PharmacyOrdersId",
                table: "OrdersDetails",
                newName: "PharmacyOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdersDetails_PharmacyOrdersId",
                table: "OrdersDetails",
                newName: "IX_OrdersDetails_PharmacyOrderId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OrdersDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drugs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateAt",
                table: "Drugs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdersDetails_PharmacysOrders_PharmacyOrderId",
                table: "OrdersDetails",
                column: "PharmacyOrderId",
                principalTable: "PharmacysOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
