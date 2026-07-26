using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onpoint.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatewishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "Wishlists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_ProductVariantId",
                table: "Wishlists",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlists_ProductVariant_ProductVariantId",
                table: "Wishlists",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlists_ProductVariant_ProductVariantId",
                table: "Wishlists");

            migrationBuilder.DropIndex(
                name: "IX_Wishlists_ProductVariantId",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "Wishlists");
        }
    }
}
