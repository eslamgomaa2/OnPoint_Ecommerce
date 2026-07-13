using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onpoint.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCouponCode",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Carts",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedCouponCode",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Carts");
        }
    }
}
