using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QotdQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QotdQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QotdMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QotdMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QotdMessage_QotdQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QotdQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordEvent_DiscordId",
                table: "DiscordEvent",
                column: "DiscordId");

            migrationBuilder.CreateIndex(
                name: "IX_QotdMessage_MessageId",
                table: "QotdMessage",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QotdMessage_QuestionId",
                table: "QotdMessage",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordEvent");

            migrationBuilder.DropTable(
                name: "QotdMessage");

            migrationBuilder.DropTable(
                name: "QotdQuestion");
        }
    }
}
