using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class UpdateBotRewriteTwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "GuildConfigs");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IgnoreNew",
                table: "WelcomeConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNsfw",
                table: "WelcomeBanners",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "ExpMultiplier",
                table: "LevelConfigs",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "VoiceExpEnabled",
                table: "LevelConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "PrefixList",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MusicConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    TextChId = table.Column<long>(nullable: true),
                    VoiceChId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Songs = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => new { x.GuildId, x.Name });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicConfigs");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropColumn(
                name: "IgnoreNew",
                table: "WelcomeConfigs");

            migrationBuilder.DropColumn(
                name: "IsNsfw",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "VoiceExpEnabled",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "PrefixList",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<int>(
                name: "ExpMultiplier",
                table: "LevelConfigs",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                nullable: true);
        }
    }
}
