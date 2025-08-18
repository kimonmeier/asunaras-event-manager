using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFeedbackMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Repeat",
                table: "EventFeedback");

            migrationBuilder.AddColumn<bool>(
                name: "Anonymous",
                table: "EventFeedback",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "FeedbackMessage",
                table: "DiscordEvent",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anonymous",
                table: "EventFeedback");

            migrationBuilder.DropColumn(
                name: "FeedbackMessage",
                table: "DiscordEvent");

            migrationBuilder.AddColumn<bool>(
                name: "Repeat",
                table: "EventFeedback",
                type: "INTEGER",
                nullable: true);
        }
    }
}
