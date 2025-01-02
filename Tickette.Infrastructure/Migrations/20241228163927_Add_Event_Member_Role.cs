using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Event_Member_Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "committee_members");

            migrationBuilder.AlterColumn<string>(
                name: "ticket_image",
                table: "tickets",
                type: "character varying(2500)",
                maxLength: 2500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "committee_role_id",
                table: "committee_members",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "committee_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_committee_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "committee_role_permissions",
                columns: table => new
                {
                    committee_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_committee_role_permissions", x => new { x.committee_role_id, x.id });
                    table.ForeignKey(
                        name: "fk_committee_role_permissions_committee_roles_committee_role_id",
                        column: x => x.committee_role_id,
                        principalTable: "committee_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_committee_members_committee_role_id",
                table: "committee_members",
                column: "committee_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_committee_roles_name",
                table: "committee_roles",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_committee_members_committee_roles_committee_role_id",
                table: "committee_members",
                column: "committee_role_id",
                principalTable: "committee_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_committee_members_committee_roles_committee_role_id",
                table: "committee_members");

            migrationBuilder.DropTable(
                name: "committee_role_permissions");

            migrationBuilder.DropTable(
                name: "committee_roles");

            migrationBuilder.DropIndex(
                name: "ix_committee_members_committee_role_id",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "committee_role_id",
                table: "committee_members");

            migrationBuilder.AlterColumn<string>(
                name: "ticket_image",
                table: "tickets",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2500)",
                oldMaxLength: 2500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "committee_members",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
