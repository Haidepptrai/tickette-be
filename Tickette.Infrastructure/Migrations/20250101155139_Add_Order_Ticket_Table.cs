using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Order_Ticket_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "remaining_tickets",
                table: "tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ticket_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_ordered_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    buyer_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    buyer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    buyer_phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    final_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_orders_users_user_ordered_id",
                        column: x => x.user_ordered_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_order_items_ticket_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "ticket_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_order_items_order_id",
                table: "ticket_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_orders_user_ordered_id",
                table: "ticket_orders",
                column: "user_ordered_id");

            migrationBuilder.CreateIndex(
                name: "IX_TicketOrder_BuyerEmail",
                table: "ticket_orders",
                column: "buyer_email");

            migrationBuilder.CreateIndex(
                name: "IX_TicketOrder_EventId",
                table: "ticket_orders",
                column: "event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_order_items");

            migrationBuilder.DropTable(
                name: "ticket_orders");

            migrationBuilder.DropColumn(
                name: "remaining_tickets",
                table: "tickets");
        }
    }
}
