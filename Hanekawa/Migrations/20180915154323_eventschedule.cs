using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class eventschedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutomaticEventSchedule",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EventPayouts",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPayouts", x => new { x.UserId, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "EventSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    Host = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedules", x => new { x.GuildId, x.Id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventPayouts");

            migrationBuilder.DropTable(
                name: "EventSchedules");

            migrationBuilder.DropColumn(
                name: "AutomaticEventSchedule",
                table: "GuildConfigs");
        }
    }
}
