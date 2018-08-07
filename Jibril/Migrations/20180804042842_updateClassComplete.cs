using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class updateClassComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "GameEnemies");

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "GameEnemies",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Class",
                table: "Accounts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "GameEnemies");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "GameEnemies",
                nullable: true);
        }
    }
}
