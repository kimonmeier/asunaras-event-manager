using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuthorFieldToQotd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "AuthorId",
                table: "QotdQuestion",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
            
            migrationBuilder.Sql("UPDATE QotdQuestion SET AuthorId = 232909611609489408");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "QotdQuestion");
        }
    }
}
