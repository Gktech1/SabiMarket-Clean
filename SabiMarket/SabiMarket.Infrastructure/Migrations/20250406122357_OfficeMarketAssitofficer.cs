/*using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OfficeMarketAssitofficer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfficerMarketAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssistCenterOfficerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficerMarketAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficerMarketAssignments_AssistCenterOfficers_AssistCenterOfficerId",
                        column: x => x.AssistCenterOfficerId,
                        principalTable: "AssistCenterOfficers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfficerMarketAssignments_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfficerMarketAssignments_AssistCenterOfficerId",
                table: "OfficerMarketAssignments",
                column: "AssistCenterOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficerMarketAssignments_MarketId",
                table: "OfficerMarketAssignments",
                column: "MarketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfficerMarketAssignments");
        }
    }
}
*/