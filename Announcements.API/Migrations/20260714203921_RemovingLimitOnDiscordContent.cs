using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Announcements.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovingLimitOnDiscordContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiscordContent",
                schema: "app",
                table: "Announcements",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiscordContent",
                schema: "app",
                table: "Announcements",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
