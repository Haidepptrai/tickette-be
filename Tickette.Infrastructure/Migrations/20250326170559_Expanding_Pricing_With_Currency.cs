using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Expanding_Pricing_With_Currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "price",
                table: "order_items");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "price_amount",
                table: "tickets",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "price_amount",
                table: "order_items",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "price_currency",
                table: "order_items",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "price_amount",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "price_amount",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "price_currency",
                table: "order_items");

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "tickets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "order_items",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
