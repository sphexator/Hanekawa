using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class customemoteadditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SuggestionEmoteYes",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "<:1yes:403870491749777411>",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuggestionEmoteNo",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "<:2no:403870492206825472>",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "SpecialCurrencySign",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "$",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialCurrencyName",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "Special Credit",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "CurrencySign",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "$",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyName",
                table: "GuildConfigs",
                nullable: true,
                defaultValue: "Credit",
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SuggestionEmoteYes",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "<:1yes:403870491749777411>");

            migrationBuilder.AlterColumn<string>(
                name: "SuggestionEmoteNo",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "<:2no:403870492206825472>");

            migrationBuilder.AlterColumn<bool>(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialCurrencySign",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "$");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialCurrencyName",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "Special Credit");

            migrationBuilder.AlterColumn<bool>(
                name: "EmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencySign",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "$");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyName",
                table: "GuildConfigs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true,
                oldDefaultValue: "Credit");
        }
    }
}
