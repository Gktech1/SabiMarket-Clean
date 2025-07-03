using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newColumnChairmanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChairmanId",
                table: "Traders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Traders_ChairmanId",
                table: "Traders",
                column: "ChairmanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Chairmen_ChairmanId",
                table: "Traders",
                column: "ChairmanId",
                principalTable: "Chairmen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Chairmen_ChairmanId",
                table: "Traders");

            migrationBuilder.DropIndex(
                name: "IX_Traders_ChairmanId",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
                table: "Traders");
        }
    }
}
