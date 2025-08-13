using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    AllowReminderInPrivateMessage = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowReminderForEvent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreference_DiscordUserId",
                table: "UserPreference",
                column: "DiscordUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreference");
        }
    }
}
