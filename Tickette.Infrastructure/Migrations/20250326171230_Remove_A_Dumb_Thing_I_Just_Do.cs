using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_A_Dumb_Thing_I_Just_Do : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "price_amount",
                table: "tickets",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_amount",
                table: "order_items",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "price_amount",
                table: "tickets",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_amount",
                table: "order_items",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");
        }
    }
}
