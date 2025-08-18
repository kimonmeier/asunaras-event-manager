using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "FeedbackMessageId",
                table: "EventFeedback",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackMessageId",
                table: "EventFeedback");
        }
    }
}
