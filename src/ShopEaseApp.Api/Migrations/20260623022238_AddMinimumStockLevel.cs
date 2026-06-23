using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopEaseApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumStockLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "ProductVariants");
        }
    }
}
