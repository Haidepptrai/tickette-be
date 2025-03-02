using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Seat_Map_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_date_event_id",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_event_id",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_row_column_event_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "column",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "event_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "is_available",
                table: "event_seats");

            migrationBuilder.RenameColumn(
                name: "row",
                table: "event_seats",
                newName: "status");

            migrationBuilder.AddColumn<string>(
                name: "attribute_height",
                table: "event_seats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "attribute_name",
                table: "event_seats",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_number",
                table: "event_seats",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_row_name",
                table: "event_seats",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_width",
                table: "event_seats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "attribute_x",
                table: "event_seats",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "attribute_y",
                table: "event_seats",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "event_seat_map_id",
                table: "event_seats",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "event_seat_maps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_date_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_seat_maps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_seat_map_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_date_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_height = table.Column<string>(type: "text", nullable: false),
                    attribute_width = table.Column<string>(type: "text", nullable: false),
                    attribute_x = table.Column<double>(type: "double precision", nullable: false),
                    attribute_y = table.Column<double>(type: "double precision", nullable: false),
                    attribute_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    attribute_row_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    attribute_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    event_seat_map_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ticket_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_seat_map_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_seat_map_sections_event_date_event_date_id",
                        column: x => x.event_date_id,
                        principalTable: "event_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_seat_map_sections_event_seat_maps_event_seat_map_id",
                        column: x => x.event_seat_map_id,
                        principalTable: "event_seat_maps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_seat_map_sections_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_seat_map_sections_tickets_ticket_id1",
                        column: x => x.ticket_id1,
                        principalTable: "tickets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_event_date_id",
                table: "event_seats",
                column: "event_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_event_seat_map_id",
                table: "event_seats",
                column: "event_seat_map_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_date_event_date_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_seat_maps_event_seat_map_id",
                table: "event_seats");

            migrationBuilder.DropTable(
                name: "event_seat_map_sections");

            migrationBuilder.DropTable(
                name: "event_seat_maps");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_event_date_id",
                table: "event_seats");

            migrationBuilder.DropIndex(
                name: "ix_event_seats_event_seat_map_id",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_height",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_name",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_number",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_row_name",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_width",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_x",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute_y",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "event_seat_map_id",
                table: "event_seats");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "event_seats",
                newName: "row");

            migrationBuilder.AddColumn<int>(
                name: "column",
                table: "event_seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "event_id",
                table: "event_seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: "event_seats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_event_id",
                table: "event_seats",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_seats_row_column_event_id",
                table: "event_seats",
                columns: new[] { "row", "column", "event_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_event_date_event_id",
                table: "event_seats",
                column: "event_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
