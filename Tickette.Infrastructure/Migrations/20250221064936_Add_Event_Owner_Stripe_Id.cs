using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickette.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Event_Owner_Stripe_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "event_owner_stripe_id",
                table: "events",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                comment: "Stripe Customer ID of the event owner");

            migrationBuilder.CreateIndex(
                name: "IX_EventOwnerStripeId",
                table: "events",
                column: "event_owner_stripe_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventOwnerStripeId",
                table: "events");

            migrationBuilder.DropColumn(
                name: "event_owner_stripe_id",
                table: "events");
        }
    }
}
