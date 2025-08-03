using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DiscordEvent",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EventRestriction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MinAlter = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxAlter = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRestriction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRestriction_DiscordEvent_DiscordEventId",
                        column: x => x.DiscordEventId,
                        principalTable: "DiscordEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRestriction_DiscordEventId_Type",
                table: "EventRestriction",
                columns: new[] { "DiscordEventId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRestriction");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DiscordEvent");
        }
    }
}
