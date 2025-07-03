using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newcolumnforlevy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Traders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketName",
                table: "Traders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBuildingTypes",
                table: "Traders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentFrequency",
                table: "Traders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "LevyPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSetupRecord",
                table: "LevyPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "LevyPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "MarketName",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "NumberOfBuildingTypes",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "PaymentFrequency",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "IsSetupRecord",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "LevyPayments");
        }
    }
}
