using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Change_Schema_Event_SeatMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_date_events_event_id",
                table: "event_date");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_map_sections_event_date_event_date_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_map_sections_event_seat_maps_event_seat_map_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_map_sections_tickets_ticket_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seat_map_sections_tickets_ticket_id1",
                table: "event_seat_map_sections");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_date_event_date_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_seat_maps_event_seat_map_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_tickets_ticket_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_event_date_event_date_id",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_seat_map_sections",
                table: "event_seat_map_sections");

            migrationBuilder.DropIndex(
                name: "ix_event_seat_map_sections_event_date_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropIndex(
                name: "ix_event_seat_map_sections_event_seat_map_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropIndex(
                name: "ix_event_seat_map_sections_ticket_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropIndex(
                name: "ix_event_seat_map_sections_ticket_id1",
                table: "event_seat_map_sections");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_seats",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_event_date_id",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_event_seat_map_id",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_ticket_id",
                table: "event_seats");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_seat_maps",
                table: "event_seat_maps");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_date",
                table: "event_date");

            migrationBuilder.DropColumn(
                name: "id",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "event_date_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "event_seat_map_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "ticket_id",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "ticket_id1",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "event_date_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "event_seat_map_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "status",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "ticket_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "id",
                table: "event_seat_maps");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "event_seat_maps");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "event_seat_maps");

            migrationBuilder.DropColumn(
                name: "event_date_id",
                table: "event_seat_maps");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "event_seat_maps");

            migrationBuilder.RenameTable(
                name: "event_seats",
                newName: "event_seat");

            migrationBuilder.RenameTable(
                name: "event_seat_maps",
                newName: "event_seat_map");

            migrationBuilder.RenameTable(
                name: "event_date",
                newName: "event_dates");

            migrationBuilder.RenameIndex(
                name: "ix_event_date_event_id",
                table: "event_dates",
                newName: "ix_event_dates_event_id");

            migrationBuilder.AddColumn<string>(
                name: "fill",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "height",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "width",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "x",
                table: "event_seat_map_sections",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "y",
                table: "event_seat_map_sections",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "fill",
                table: "event_seat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "height",
                table: "event_seat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_ordered",
                table: "event_seat",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "event_seat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "row_name",
                table: "event_seat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "width",
                table: "event_seat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "x",
                table: "event_seat",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "y",
                table: "event_seat",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "seat_map",
                table: "event_dates",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_dates",
                table: "event_dates",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_dates_events_event_id",
                table: "event_dates",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_event_dates_event_date_id",
                table: "tickets",
                column: "event_date_id",
                principalTable: "event_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_dates_events_event_id",
                table: "event_dates");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_event_dates_event_date_id",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_dates",
                table: "event_dates");

            migrationBuilder.DropColumn(
                name: "fill",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "height",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "name",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "type",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "width",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "x",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "y",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "fill",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "height",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "is_ordered",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "number",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "row_name",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "width",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "x",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "y",
                table: "event_seat");

            migrationBuilder.DropColumn(
                name: "seat_map",
                table: "event_dates");

            migrationBuilder.RenameTable(
                name: "event_seat_map",
                newName: "event_seat_maps");

            migrationBuilder.RenameTable(
                name: "event_seat",
                newName: "event_seats");

            migrationBuilder.RenameTable(
                name: "event_dates",
                newName: "event_date");

            migrationBuilder.RenameIndex(
                name: "ix_event_dates_event_id",
                table: "event_date",
                newName: "ix_event_date_event_id");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "event_seat_map_sections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "attribute",
                table: "event_seat_map_sections",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "event_seat_map_sections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "event_seat_map_sections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "event_date_id",
                table: "event_seat_map_sections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "event_seat_map_id",
                table: "event_seat_map_sections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "event_seat_map_sections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ticket_id",
                table: "event_seat_map_sections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ticket_id1",
                table: "event_seat_map_sections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "event_seat_map_sections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "event_seat_maps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "event_seat_maps",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "event_seat_maps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "event_date_id",
                table: "event_seat_maps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "event_seat_maps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "event_seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "attribute",
                table: "event_seats",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "event_seats",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "event_seats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "event_date_id",
                table: "event_seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "event_seat_map_id",
                table: "event_seats",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "event_seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ticket_id",
                table: "event_seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "event_seats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_seat_map_sections",
                table: "event_seat_map_sections",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_seat_maps",
                table: "event_seat_maps",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_seats",
                table: "event_seats",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_date",
                table: "event_date",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seat_map_sections_event_date_id",
                table: "event_seat_map_sections",
                column: "event_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seat_map_sections_event_seat_map_id",
                table: "event_seat_map_sections",
                column: "event_seat_map_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seat_map_sections_ticket_id",
                table: "event_seat_map_sections",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seat_map_sections_ticket_id1",
                table: "event_seat_map_sections",
                column: "ticket_id1");

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_event_date_id",
                table: "event_seats",
                column: "event_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_event_seat_map_id",
                table: "event_seats",
                column: "event_seat_map_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_ticket_id",
                table: "event_seats",
                column: "ticket_id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_date_events_event_id",
                table: "event_date",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_map_sections_event_date_event_date_id",
                table: "event_seat_map_sections",
                column: "event_date_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_map_sections_event_seat_maps_event_seat_map_id",
                table: "event_seat_map_sections",
                column: "event_seat_map_id",
                principalTable: "event_seat_maps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_map_sections_tickets_ticket_id",
                table: "event_seat_map_sections",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seat_map_sections_tickets_ticket_id1",
                table: "event_seat_map_sections",
                column: "ticket_id1",
                principalTable: "tickets",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_event_date_event_date_id",
                table: "event_seats",
                column: "event_date_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_event_seat_maps_event_seat_map_id",
                table: "event_seats",
                column: "event_seat_map_id",
                principalTable: "event_seat_maps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_tickets_ticket_id",
                table: "event_seats",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_event_date_event_date_id",
                table: "tickets",
                column: "event_date_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
