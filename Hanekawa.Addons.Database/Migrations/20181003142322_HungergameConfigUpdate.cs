using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class HungergameConfigUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinCredit",
                table: "HungerGameConfigs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinExp",
                table: "HungerGameConfigs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinSpecialCredit",
                table: "HungerGameConfigs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<ulong>(
                name: "WinnerRoleId",
                table: "HungerGameConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinCredit",
                table: "HungerGameConfigs");

            migrationBuilder.DropColumn(
                name: "WinExp",
                table: "HungerGameConfigs");

            migrationBuilder.DropColumn(
                name: "WinSpecialCredit",
                table: "HungerGameConfigs");

            migrationBuilder.DropColumn(
                name: "WinnerRoleId",
                table: "HungerGameConfigs");
        }
    }
}
