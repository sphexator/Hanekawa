using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class EventScheduleFixPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules",
                columns: new[] { "Id", "GuildId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules",
                columns: new[] { "GuildId", "Id" });
        }
    }
}
