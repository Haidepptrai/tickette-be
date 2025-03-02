using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modify_ED_SeatMap_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_seat");

            migrationBuilder.DropTable(
                name: "event_seat_map");

            migrationBuilder.DropTable(
                name: "event_seat_map_sections");

            migrationBuilder.CreateIndex(
                name: "ix_event_dates_seat_map",
                table: "event_dates",
                column: "seat_map")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_event_dates_seat_map",
                table: "event_dates");

            migrationBuilder.CreateTable(
                name: "event_seat",
                columns: table => new
                {
                    fill = table.Column<string>(type: "text", nullable: false),
                    height = table.Column<string>(type: "text", nullable: false),
                    is_ordered = table.Column<bool>(type: "boolean", nullable: false),
                    number = table.Column<string>(type: "text", nullable: false),
                    row_name = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<string>(type: "text", nullable: false),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "event_seat_map",
                columns: table => new
                {
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "event_seat_map_sections",
                columns: table => new
                {
                    fill = table.Column<string>(type: "text", nullable: false),
                    height = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<string>(type: "text", nullable: false),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
