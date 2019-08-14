using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class UpdateBotRewriteThree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSchedules");

            migrationBuilder.DropTable(
                name: "NudeServiceChannels");

            migrationBuilder.DropTable(
                name: "ProfileConfigs");

            migrationBuilder.DropTable(
                name: "QuestionAndAnswers");

            migrationBuilder.DropTable(
                name: "SingleNudeServiceChannels");

            migrationBuilder.DropTable(
                name: "SpamIgnores");

            migrationBuilder.DropTable(
                name: "UrlFilters");

            migrationBuilder.DropTable(
                name: "WhitelistDesigns");

            migrationBuilder.DropTable(
                name: "WhitelistEvents");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "LevelExpReductions");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "LevelExpReductions");

            migrationBuilder.DropColumn(
                name: "PrefixList",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "ExpMultiplier",
                table: "LevelConfigs",
                newName: "VoiceExpMultiplier");

            migrationBuilder.AddColumn<int>(
                name: "Reward",
                table: "WelcomeConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "LevelExpReductions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "Multiplier",
                table: "LevelExpEvents",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "ExpDisabled",
                table: "LevelConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TextExpEnabled",
                table: "LevelConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TextExpMultiplier",
                table: "LevelConfigs",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<long>(
                name: "Role",
                table: "Items",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Items",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<int>(
                name: "CriticalIncrease",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DamageIncrease",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HealthIncrease",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sell",
                table: "Items",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Items",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApprovalQueues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Uploader = table.Column<long>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UploadTimeOffset = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalQueues", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Highlights",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Highlights = table.Column<string[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => new { x.GuildId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalQueues");

            migrationBuilder.DropTable(
                name: "Highlights");

            migrationBuilder.DropColumn(
                name: "Reward",
                table: "WelcomeConfigs");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "LevelExpReductions");

            migrationBuilder.DropColumn(
                name: "ExpDisabled",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "TextExpEnabled",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "TextExpMultiplier",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CriticalIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DamageIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "HealthIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Sell",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "VoiceExpMultiplier",
                table: "LevelConfigs",
                newName: "ExpMultiplier");

            migrationBuilder.AddColumn<bool>(
                name: "Category",
                table: "LevelExpReductions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Channel",
                table: "LevelExpReductions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Multiplier",
                table: "LevelExpEvents",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<long>(
                name: "Role",
                table: "Items",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Items",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "PrefixList",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DesignerClaim = table.Column<long>(nullable: true),
                    Host = table.Column<long>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Posted = table.Column<bool>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedules", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "NudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    InHouse = table.Column<bool>(nullable: false),
                    Tolerance = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NudeServiceChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "ProfileConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Height = table.Column<float>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NameWidth = table.Column<float>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    ValueWidth = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAndAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    MessageId = table.Column<long>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    ResponseUser = table.Column<long>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAndAnswers", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "SingleNudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    InHouse = table.Column<bool>(nullable: false),
                    Level = table.Column<int>(nullable: true),
                    Tolerance = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleNudeServiceChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "SpamIgnores",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpamIgnores", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "UrlFilters",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFilters", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "WhitelistDesigns",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistDesigns", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "WhitelistEvents",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistEvents", x => new { x.GuildId, x.UserId });
                });
        }
    }
}
