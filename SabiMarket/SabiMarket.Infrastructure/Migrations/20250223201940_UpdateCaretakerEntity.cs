using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCaretakerEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaretakerId1",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Markets_CaretakerId1",
                table: "Markets",
                column: "CaretakerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Markets_Caretakers_CaretakerId1",
                table: "Markets",
                column: "CaretakerId1",
                principalTable: "Caretakers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Markets_Caretakers_CaretakerId1",
                table: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Markets_CaretakerId1",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "CaretakerId1",
                table: "Markets");
        }
    }
}
