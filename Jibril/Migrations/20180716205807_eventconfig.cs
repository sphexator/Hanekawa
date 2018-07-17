using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class eventconfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "EventChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "EventSchedulerChannel",
                table: "GuildConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "EventSchedulerChannel",
                table: "GuildConfigs");
        }
    }
}
