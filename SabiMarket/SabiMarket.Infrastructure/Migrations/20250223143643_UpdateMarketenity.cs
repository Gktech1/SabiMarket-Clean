using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketenity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_WaivedProducts_Vendors_VendorId",
                table: "WaivedProducts");

            migrationBuilder.DropTable(
                name: "ProductCategoryWaivedProduct");

            migrationBuilder.DropIndex(
                name: "IX_Markets_ChairmanId",
                table: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Caretakers_MarketId",
                table: "Caretakers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WaivedProducts");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "WaivedProducts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Markets");

            migrationBuilder.RenameColumn(
                name: "WaivedPrice",
                table: "WaivedProducts",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "WaivedProducts",
                newName: "CurrencyType");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "WaivedProducts",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "WaivedProducts",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "ProductCategoryId",
                table: "WaivedProducts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MarketName",
                table: "Markets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LocalGovernmentName",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LocalGovernmentId",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ChairmanId",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CaretakerId",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketType",
                table: "Markets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MarketId",
                table: "Caretakers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "LocalGovernmentId",
                table: "Caretakers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaivedProducts_ProductCategoryId",
                table: "WaivedProducts",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_CaretakerId",
                table: "Markets",
                column: "CaretakerId");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_ChairmanId",
                table: "Markets",
                column: "ChairmanId",
                unique: true,
                filter: "[ChairmanId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Caretakers_LocalGovernmentId",
                table: "Caretakers",
                column: "LocalGovernmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_LocalGovernments_LocalGovernmentId",
                table: "Caretakers",
                column: "LocalGovernmentId",
                principalTable: "LocalGovernments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Markets_Caretakers_CaretakerId",
                table: "Markets",
                column: "CaretakerId",
                principalTable: "Caretakers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WaivedProducts_ProductCategories_ProductCategoryId",
                table: "WaivedProducts",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WaivedProducts_Vendors_VendorId",
                table: "WaivedProducts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_LocalGovernments_LocalGovernmentId",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Markets_Caretakers_CaretakerId",
                table: "Markets");

            migrationBuilder.DropForeignKey(
                name: "FK_WaivedProducts_ProductCategories_ProductCategoryId",
                table: "WaivedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_WaivedProducts_Vendors_VendorId",
                table: "WaivedProducts");

            migrationBuilder.DropIndex(
                name: "IX_WaivedProducts_ProductCategoryId",
                table: "WaivedProducts");

            migrationBuilder.DropIndex(
                name: "IX_Markets_CaretakerId",
                table: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Markets_ChairmanId",
                table: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Caretakers_LocalGovernmentId",
                table: "Caretakers");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "WaivedProducts");

            migrationBuilder.DropColumn(
                name: "CaretakerId",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "MarketType",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "LocalGovernmentId",
                table: "Caretakers");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "WaivedProducts",
                newName: "ProductCode");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "WaivedProducts",
                newName: "WaivedPrice");

            migrationBuilder.RenameColumn(
                name: "CurrencyType",
                table: "WaivedProducts",
                newName: "StockQuantity");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "WaivedProducts",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WaivedProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "WaivedProducts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "MarketName",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocalGovernmentName",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LocalGovernmentId",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChairmanId",
                table: "Markets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Markets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "MarketId",
                table: "Caretakers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ProductCategoryWaivedProduct",
                columns: table => new
                {
                    CategoriesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryWaivedProduct", x => new { x.CategoriesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductCategoryWaivedProduct_ProductCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategoryWaivedProduct_WaivedProducts_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "WaivedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Markets_ChairmanId",
                table: "Markets",
                column: "ChairmanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Caretakers_MarketId",
                table: "Caretakers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryWaivedProduct_ProductsId",
                table: "ProductCategoryWaivedProduct",
                column: "ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                table: "Caretakers",
                column: "MarketId",
                principalTable: "Markets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WaivedProducts_Vendors_VendorId",
                table: "WaivedProducts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }
    }
}
