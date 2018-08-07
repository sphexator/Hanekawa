using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class test123 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Rare",
                table: "GameEnemies",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<bool>(
                name: "Elite",
                table: "GameEnemies",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<int>(
                name: "DefaultHealth",
                table: "GameConfigs",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDamage",
                table: "GameConfigs",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Rare",
                table: "GameEnemies",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Elite",
                table: "GameEnemies",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultHealth",
                table: "GameConfigs",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDamage",
                table: "GameConfigs",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: 10);
        }
    }
}
