using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GoodBoysRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                table: "GoodBoys");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                table: "GoodBoys",
                column: "CaretakerId",
                principalTable: "Caretakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                table: "GoodBoys");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                table: "GoodBoys",
                column: "CaretakerId",
                principalTable: "Caretakers",
                principalColumn: "Id");
        }
    }
}
