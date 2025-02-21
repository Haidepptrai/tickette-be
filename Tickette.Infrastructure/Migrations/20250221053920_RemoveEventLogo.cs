using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "logo",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logo",
                table: "events",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
