using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Committee_Member_Table_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_committee_members",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "id",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "joined_at",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "committee_members");

            migrationBuilder.AddColumn<Guid>(
                name: "committee_role_id1",
                table: "committee_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_committee_members",
                table: "committee_members",
                columns: new[] { "user_id", "event_id" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_committee_members_committee_roles_committee_role_id1",
                table: "committee_members");

            migrationBuilder.DropPrimaryKey(
                name: "pk_committee_members",
                table: "committee_members");

            migrationBuilder.DropIndex(
                name: "ix_committee_members_committee_role_id1",
                table: "committee_members");

            migrationBuilder.DropColumn(
                name: "committee_role_id1",
                table: "committee_members");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "committee_members",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "committee_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "joined_at",
                table: "committee_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "committee_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_committee_members",
                table: "committee_members",
                column: "id");
        }
    }
}
