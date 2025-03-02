using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Seat_And_Section_Attributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "attribute_height",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_name",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_number",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_row_name",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_width",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_x",
                table: "event_seat_map_sections");

            migrationBuilder.DropColumn(
                name: "attribute_y",
                table: "event_seat_map_sections");

            migrationBuilder.AddColumn<string>(
                name: "attribute",
                table: "event_seats",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "attribute",
                table: "event_seat_map_sections",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attribute",
                table: "event_seats");

            migrationBuilder.DropColumn(
                name: "attribute",
                table: "event_seat_map_sections");

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

            migrationBuilder.AddColumn<string>(
                name: "attribute_height",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "attribute_name",
                table: "event_seat_map_sections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_number",
                table: "event_seat_map_sections",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_row_name",
                table: "event_seat_map_sections",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attribute_width",
                table: "event_seat_map_sections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "attribute_x",
                table: "event_seat_map_sections",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "attribute_y",
                table: "event_seat_map_sections",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
