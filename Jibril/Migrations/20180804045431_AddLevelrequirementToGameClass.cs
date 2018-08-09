using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class AddLevelrequirementToGameClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "LevelRequirement",
                table: "GameClasses",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<int>(
                name: "Class",
                table: "Accounts",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelRequirement",
                table: "GameClasses");

            migrationBuilder.AlterColumn<int>(
                name: "Class",
                table: "Accounts",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: 1);
        }
    }
}
