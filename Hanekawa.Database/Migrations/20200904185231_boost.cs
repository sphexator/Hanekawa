using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class boost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BoostExpMultiplier",
                table: "LevelConfigs",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.CreateTable(
                name: "BoostConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    CreditGain = table.Column<int>(nullable: false),
                    SpecialCreditGain = table.Column<int>(nullable: false),
                    ExpGain = table.Column<int>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_BoostConfigs", x => x.GuildId); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoostConfigs");

            migrationBuilder.DropColumn(
                name: "BoostExpMultiplier",
                table: "LevelConfigs");
        }
    }
}