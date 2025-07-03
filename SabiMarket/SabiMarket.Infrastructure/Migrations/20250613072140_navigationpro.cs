using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class navigationpro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MarketId",
                table: "LevySetup",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "LevySetup",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevySetup_MarketId",
                table: "LevySetup",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_LevySetup_UserId",
                table: "LevySetup",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LevySetup_AspNetUsers_UserId",
                table: "LevySetup",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LevySetup_Markets_MarketId",
                table: "LevySetup",
                column: "MarketId",
                principalTable: "Markets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LevySetup_AspNetUsers_UserId",
                table: "LevySetup");

            migrationBuilder.DropForeignKey(
                name: "FK_LevySetup_Markets_MarketId",
                table: "LevySetup");

            migrationBuilder.DropIndex(
                name: "IX_LevySetup_MarketId",
                table: "LevySetup");

            migrationBuilder.DropIndex(
                name: "IX_LevySetup_UserId",
                table: "LevySetup");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "LevySetup");

            migrationBuilder.AlterColumn<string>(
                name: "MarketId",
                table: "LevySetup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
