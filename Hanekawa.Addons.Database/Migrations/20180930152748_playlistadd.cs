using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class playlistadd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPayouts",
                table: "EventPayouts");

            migrationBuilder.AlterColumn<int>(
                name: "Water",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "TotalWeapons",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Thirst",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Stamina",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Sleep",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Pistol",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Hunger",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Health",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Food",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Fatigue",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Bow",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Axe",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AddColumn<ulong>(
                name: "DesignChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "DesignerClaim",
                table: "EventSchedules",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPayouts",
                table: "EventPayouts",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BackgroundUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BanStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanStats", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Unique = table.Column<bool>(nullable: false),
                    StackAble = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "JoinStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LatestEntry = table.Column<DateTime>(nullable: false),
                    Join = table.Column<int>(nullable: false),
                    Leave = table.Column<int>(nullable: false),
                    DayOne = table.Column<int>(nullable: false),
                    DayOneDate = table.Column<DateTime>(nullable: false),
                    DayTwo = table.Column<int>(nullable: false),
                    DayTwoDate = table.Column<DateTime>(nullable: false),
                    DayThree = table.Column<int>(nullable: false),
                    DayThreeDate = table.Column<DateTime>(nullable: false),
                    DayFour = table.Column<int>(nullable: false),
                    DayFourDate = table.Column<DateTime>(nullable: false),
                    DayFive = table.Column<int>(nullable: false),
                    DayFiveDate = table.Column<DateTime>(nullable: false),
                    DaySix = table.Column<int>(nullable: false),
                    DaySixDate = table.Column<DateTime>(nullable: false),
                    DaySeven = table.Column<int>(nullable: false),
                    DaySevenDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinStats", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "MessageStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Monday = table.Column<int>(nullable: false),
                    Tuesday = table.Column<int>(nullable: false),
                    Wednesday = table.Column<int>(nullable: false),
                    Thursday = table.Column<int>(nullable: false),
                    Friday = table.Column<int>(nullable: false),
                    Saturday = table.Column<int>(nullable: false),
                    Sunday = table.Column<int>(nullable: false),
                    WeekOne = table.Column<string>(nullable: true),
                    WeekTwo = table.Column<string>(nullable: true),
                    WeekThree = table.Column<string>(nullable: true),
                    WeekFour = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageStats", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "MuteStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuteStats", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    Streams = table.Column<int>(nullable: false),
                    IsPrivate = table.Column<bool>(nullable: false),
                    OwnerId = table.Column<ulong>(nullable: false),
                    Playtime = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => new { x.GuildId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "ProfileConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Height = table.Column<float>(nullable: false),
                    NameWidth = table.Column<float>(nullable: false),
                    ValueWidth = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarnStats",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarnStats", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "WhitelistDesigns",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistDesigns", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "WhitelistEvents",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistEvents", x => new { x.GuildId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "BanStats");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "JoinStats");

            migrationBuilder.DropTable(
                name: "MessageStats");

            migrationBuilder.DropTable(
                name: "MuteStats");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "ProfileConfigs");

            migrationBuilder.DropTable(
                name: "WarnStats");

            migrationBuilder.DropTable(
                name: "WhitelistDesigns");

            migrationBuilder.DropTable(
                name: "WhitelistEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPayouts",
                table: "EventPayouts");

            migrationBuilder.DropColumn(
                name: "DesignChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "DesignerClaim",
                table: "EventSchedules");

            migrationBuilder.AlterColumn<uint>(
                name: "Water",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "TotalWeapons",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Thirst",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Stamina",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Sleep",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Pistol",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Hunger",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Health",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Food",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Fatigue",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Bow",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "Axe",
                table: "HungerGameLives",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "UserId", "GuildId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPayouts",
                table: "EventPayouts",
                columns: new[] { "UserId", "GuildId" });
        }
    }
}
