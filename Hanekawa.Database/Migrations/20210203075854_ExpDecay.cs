using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class ExpDecay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoDecay",
                table: "LevelRewards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Decay",
                table: "LevelConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Decay",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoDecay",
                table: "LevelRewards");

            migrationBuilder.DropColumn(
                name: "Decay",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "Decay",
                table: "Accounts");
        }
    }
}
