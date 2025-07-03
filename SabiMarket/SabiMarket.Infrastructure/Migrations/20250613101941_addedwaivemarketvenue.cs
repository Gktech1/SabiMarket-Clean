using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedwaivemarketvenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NextWaiveMarket",
                table: "WaiveMarketDates",
                newName: "NextWaiveMarketDate");

            migrationBuilder.AddColumn<string>(
                name: "WaiveMarketLocation",
                table: "WaiveMarketDates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaiveMarketLocation",
                table: "WaiveMarketDates");

            migrationBuilder.RenameColumn(
                name: "NextWaiveMarketDate",
                table: "WaiveMarketDates",
                newName: "NextWaiveMarket");
        }
    }
}
