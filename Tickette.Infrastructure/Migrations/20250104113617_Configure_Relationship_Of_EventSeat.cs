using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Configure_Relationship_Of_EventSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_tickets_ticket_id",
                table: "event_seat");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_seat",
                table: "event_seat");

            migrationBuilder.RenameTable(
                name: "event_seat",
                newName: "event_seats");

            migrationBuilder.RenameIndex(
                name: "ix_event_seat_ticket_id",
                table: "event_seats",
                newName: "ix_event_seats_ticket_id");

            migrationBuilder.RenameIndex(
                name: "ix_event_seat_row_column_event_id",
                table: "event_seats",
                newName: "ix_event_seats_row_column_event_id");

            migrationBuilder.RenameIndex(
                name: "ix_event_seat_event_id",
                table: "event_seats",
                newName: "ix_event_seats_event_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_seats",
                table: "event_seats",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_ordered_id",
                table: "orders",
                column: "user_ordered_id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_events_event_id",
                table: "event_seats",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_tickets_ticket_id",
                table: "event_seats",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_events_event_id",
                table: "orders",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_ordered_id",
                table: "orders",
                column: "user_ordered_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_events_event_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_tickets_ticket_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_events_event_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_ordered_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_user_ordered_id",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_seats",
                table: "event_seats");

            migrationBuilder.RenameTable(
                name: "event_seats",
                newName: "event_seat");

            migrationBuilder.RenameIndex(
                name: "ix_event_seats_ticket_id",
                table: "event_seat",
                newName: "ix_event_seat_ticket_id");

            migrationBuilder.RenameIndex(
                name: "ix_event_seats_row_column_event_id",
                table: "event_seat",
                newName: "ix_event_seat_row_column_event_id");

            migrationBuilder.RenameIndex(
                name: "ix_event_seats_event_id",
                table: "event_seat",
                newName: "ix_event_seat_event_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_seat",
                table: "event_seat",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_events_event_id",
                table: "event_seat",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_tickets_ticket_id",
                table: "event_seat",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
