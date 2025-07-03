using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LevySetupEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevySetup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChairmanId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MarketId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentFrequency = table.Column<int>(type: "int", nullable: false),
                    OccupancyType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSetupRecord = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevySetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LevySetup_Chairmen_ChairmanId",
                        column: x => x.ChairmanId,
                        principalTable: "Chairmen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LevySetup_ChairmanId",
                table: "LevySetup",
                column: "ChairmanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevySetup");
        }
    }
}
