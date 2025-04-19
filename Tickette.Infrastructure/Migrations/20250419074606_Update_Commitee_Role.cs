using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Commitee_Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_committee_members_committee_roles_committee_role_id1",
                table: "committee_members");

            migrationBuilder.DropTable(
                name: "committee_role_permissions");

            migrationBuilder.DropIndex(
                name: "ix_committee_members_committee_role_id1",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "committee_role_id1",
                table: "committee_members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "committee_role_id1",
                table: "committee_members",
                type: "uuid",
                nullable: true);

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
                name: "ix_committee_members_committee_role_id1",
                table: "committee_members",
                column: "committee_role_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_committee_members_committee_roles_committee_role_id1",
                table: "committee_members",
                column: "committee_role_id1",
                principalTable: "committee_roles",
                principalColumn: "id");
        }
    }
}
