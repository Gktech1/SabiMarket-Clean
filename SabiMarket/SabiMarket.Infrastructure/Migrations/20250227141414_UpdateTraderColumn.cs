using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTraderColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TraderOccupancy",
                table: "Traders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "CustomerFeedbacks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerPurchases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WaivedProductId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeliveryInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProofOfPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPaymentConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPurchases_WaivedProducts_WaivedProductId",
                        column: x => x.WaivedProductId,
                        principalTable: "WaivedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchases_WaivedProductId",
                table: "CustomerPurchases",
                column: "WaivedProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPurchases");

            migrationBuilder.DropColumn(
                name: "TraderOccupancy",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "CustomerFeedbacks");
        }
    }
}
