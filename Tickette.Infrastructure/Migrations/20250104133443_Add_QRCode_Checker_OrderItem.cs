using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_QRCode_Checker_OrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "order_items",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<bool>(
                name: "is_scanned",
                table: "order_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "scanned_at",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_scanned",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "scanned_at",
                table: "order_items");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "order_items",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");
        }
    }
}
