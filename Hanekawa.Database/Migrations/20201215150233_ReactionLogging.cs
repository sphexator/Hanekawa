using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class ReactionLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LogReaction",
                table: "LoggingConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReactionWebhook",
                table: "LoggingConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogReaction",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "ReactionWebhook",
                table: "LoggingConfigs");
        }
    }
}
