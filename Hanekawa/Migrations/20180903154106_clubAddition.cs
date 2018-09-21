using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class clubAddition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ClubPlayers");

            migrationBuilder.AlterColumn<bool>(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<bool>(
                name: "EmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AddColumn<ulong>(
                name: "ClubAdvertisementChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "ClubChannelCategory",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "ClubChannelRequiredAmount",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "ClubChannelRequiredLevel",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "ClubEnableVoiceChannel",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ClubId",
                table: "ClubPlayers",
                nullable: false,
                oldClrType: typeof(uint))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ClubPlayers",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "ClubPlayers",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "ClubInfos",
                nullable: true,
                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ClubInfos",
                nullable: false,
                oldClrType: typeof(uint))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "AdMessage",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoAdd",
                table: "ClubInfos",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Public",
                table: "ClubInfos",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers",
                columns: new[] { "Id", "ClubId", "GuildId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                columns: new[] { "GuildId", "Id", "Leader" });

            migrationBuilder.CreateTable(
                name: "ClubBlacklists",
                columns: table => new
                {
                    ClubId = table.Column<int>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    BlackListUser = table.Column<ulong>(nullable: false),
                    IssuedUser = table.Column<ulong>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Time = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubBlacklists", x => new { x.ClubId, x.GuildId, x.BlackListUser });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubBlacklists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "ClubAdvertisementChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ClubChannelCategory",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ClubChannelRequiredAmount",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ClubChannelRequiredLevel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "ClubEnableVoiceChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ClubPlayers");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "ClubPlayers");

            migrationBuilder.DropColumn(
                name: "AdMessage",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "AutoAdd",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "Public",
                table: "ClubInfos");

            migrationBuilder.AlterColumn<bool>(
                name: "SpecialEmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "EmoteCurrency",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<uint>(
                name: "ClubId",
                table: "ClubPlayers",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ClubPlayers",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "ClubInfos",
                nullable: false,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "ClubInfos",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers",
                column: "ClubId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                column: "Id");
        }
    }
}
