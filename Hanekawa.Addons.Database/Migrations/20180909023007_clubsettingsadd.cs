using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class clubsettingsadd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos");

            migrationBuilder.AddColumn<bool>(
                name: "ClubAutoPrune",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ClubPlayers",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "InactiveTime",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                columns: new[] { "Id", "GuildId", "Leader" });

            migrationBuilder.CreateTable(
                name: "LevelExpEvents",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<ulong>(nullable: true),
                    MessageId = table.Column<ulong>(nullable: true),
                    Multiplier = table.Column<uint>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpEvents", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevelExpEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "ClubAutoPrune",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "InactiveTime",
                table: "ClubInfos");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ClubPlayers",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                columns: new[] { "GuildId", "Id", "Leader" });
        }
    }
}
