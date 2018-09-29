using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class guildconfigupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyName",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySign",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FilterAllInv",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "FilterMsgLength",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FilterUrls",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "LogWarn",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WelcomeDelete",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "Credit",
                table: "AccountGlobals",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "StarGive",
                table: "AccountGlobals",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "StarReceive",
                table: "AccountGlobals",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyName",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "CurrencySign",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "EmoteCurrency",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "FilterAllInv",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "FilterMsgLength",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "FilterUrls",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogWarn",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "WelcomeDelete",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "AccountGlobals");

            migrationBuilder.DropColumn(
                name: "StarGive",
                table: "AccountGlobals");

            migrationBuilder.DropColumn(
                name: "StarReceive",
                table: "AccountGlobals");
        }
    }
}
