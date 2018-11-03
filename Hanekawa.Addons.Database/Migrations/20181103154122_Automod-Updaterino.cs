using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class AutomodUpdaterino : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmoteCountFilter",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "LogAutoMod",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MentionCountFilter",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SingleNudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Tolerance = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleNudeServiceChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "SpamIgnores",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpamIgnores", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "UrlFilters",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFilters", x => new { x.GuildId, x.ChannelId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleNudeServiceChannels");

            migrationBuilder.DropTable(
                name: "SpamIgnores");

            migrationBuilder.DropTable(
                name: "UrlFilters");

            migrationBuilder.DropColumn(
                name: "EmoteCountFilter",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogAutoMod",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MentionCountFilter",
                table: "GuildConfigs");
        }
    }
}
