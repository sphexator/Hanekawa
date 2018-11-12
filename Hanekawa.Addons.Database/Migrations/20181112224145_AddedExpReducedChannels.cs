using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class AddedExpReducedChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevelExpReductions",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    Channel = table.Column<bool>(nullable: false),
                    Category = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpReductions", x => new { x.GuildId, x.ChannelId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevelExpReductions");
        }
    }
}
