using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class customCurrencyBoardSuggestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BoardEmote",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialCurrencyName",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialCurrencySign",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Boarded",
                table: "Boards",
                nullable: true,
                oldClrType: typeof(DateTimeOffset));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialCurrencyName",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "SpecialCurrencySign",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<ulong>(
                name: "BoardEmote",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Boarded",
                table: "Boards",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);
        }
    }
}
