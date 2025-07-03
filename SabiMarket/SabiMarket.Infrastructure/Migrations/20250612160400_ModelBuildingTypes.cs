using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelBuildingTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfBuildingTypes",
                table: "Traders");

            migrationBuilder.CreateTable(
                name: "TraderBuildingTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TraderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BuildingType = table.Column<int>(type: "int", nullable: false),
                    NumberOfBuildingTypes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderBuildingTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraderBuildingTypes_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraderBuildingTypes_TraderId",
                table: "TraderBuildingTypes",
                column: "TraderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraderBuildingTypes");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBuildingTypes",
                table: "Traders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
