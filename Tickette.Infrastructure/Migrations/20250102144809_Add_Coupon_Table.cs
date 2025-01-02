using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Coupon_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_ordered_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_user_ordered_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "final_price",
                table: "orders");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_price",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<Guid>(
                name: "coupon_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "original_price",
                table: "orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "order_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coupons", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coupon_EventId_Code",
                table: "coupons",
                columns: new[] { "event_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropColumn(
                name: "coupon_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "original_price",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "price",
                table: "order_items");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_price",
                table: "orders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "final_price",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_ordered_id",
                table: "orders",
                column: "user_ordered_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_ordered_id",
                table: "orders",
                column: "user_ordered_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
