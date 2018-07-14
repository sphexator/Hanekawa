using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class boardSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "BoardChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LootChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootChannels", x => new { x.GuildId, x.ChannelId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LootChannels");

            migrationBuilder.DropColumn(
                name: "BoardChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "GuildConfigs");
        }
    }
}
