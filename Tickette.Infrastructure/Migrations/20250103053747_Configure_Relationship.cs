using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Configure_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_ticket_id",
                table: "order_items",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seat_row_column_event_id",
                table: "event_seat",
                columns: new[] { "row", "column", "event_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_tickets_ticket_id",
                table: "order_items",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat");

            migrationBuilder.DropForeignKey(
                name: "fk_order_items_tickets_ticket_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_order_items_ticket_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_event_seat_row_column_event_id",
                table: "event_seat");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
