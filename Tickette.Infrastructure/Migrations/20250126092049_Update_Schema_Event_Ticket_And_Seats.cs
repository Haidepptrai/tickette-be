using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Schema_Event_Ticket_And_Seats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_events_event_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_events_event_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "refresh_token",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "refresh_token_expiry_time",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "address",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "tickets",
                newName: "event_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_tickets_event_id",
                table: "tickets",
                newName: "ix_tickets_event_date_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10000)",
                oldMaxLength: 10000);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_id",
                table: "events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "district",
                table: "events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "location_name",
                table: "events",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "street_address",
                table: "events",
                type: "character varying(350)",
                maxLength: 350,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ward",
                table: "events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "event_date_id",
                table: "event_seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "event_date",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_date", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_date_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expiry_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_created_by_id",
                table: "events",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_date_event_id",
                table: "event_date",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_event_date_event_id",
                table: "event_seats",
                column: "event_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_events_asp_net_users_created_by_id",
                table: "events",
                column: "created_by_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_event_date_event_date_id",
                table: "tickets",
                column: "event_date_id",
                principalTable: "event_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_seats_event_date_event_id",
                table: "event_seats");

            migrationBuilder.DropForeignKey(
                name: "fk_events_asp_net_users_created_by_id",
                table: "events");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_event_date_event_date_id",
                table: "tickets");

            migrationBuilder.DropTable(
                name: "event_date");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_events_created_by_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "city",
                table: "events");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "district",
                table: "events");

            migrationBuilder.DropColumn(
                name: "location_name",
                table: "events");

            migrationBuilder.DropColumn(
                name: "street_address",
                table: "events");

            migrationBuilder.DropColumn(
                name: "ward",
                table: "events");

            migrationBuilder.DropColumn(
                name: "event_date_id",
                table: "event_seats");

            migrationBuilder.RenameColumn(
                name: "event_date_id",
                table: "tickets",
                newName: "event_id");

            migrationBuilder.RenameIndex(
                name: "ix_tickets_event_date_id",
                table: "tickets",
                newName: "ix_tickets_event_id");

            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                table: "identity_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "refresh_token_expiry_time",
                table: "identity_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "events",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "events",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_event_seats_events_event_id",
                table: "event_seats",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_events_event_id",
                table: "tickets",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
