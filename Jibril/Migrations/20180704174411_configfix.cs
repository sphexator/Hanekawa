using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class configfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Welcome",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<ulong>(
                name: "WelcomeChannel",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(ulong));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "WelcomeChannel",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Welcome",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);
        }
    }
}
