using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Repeat = table.Column<bool>(type: "INTEGER", nullable: true),
                    Good = table.Column<string>(type: "TEXT", nullable: true),
                    Critic = table.Column<string>(type: "TEXT", nullable: true),
                    Suggestion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventFeedback_DiscordEvent_DiscordEventId",
                        column: x => x.DiscordEventId,
                        principalTable: "DiscordEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventFeedback_DiscordEventId_UserId",
                table: "EventFeedback",
                columns: new[] { "DiscordEventId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventFeedback");
        }
    }
}
