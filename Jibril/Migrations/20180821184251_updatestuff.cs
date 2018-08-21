using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class updatestuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InviteMessageId",
                table: "GuildInfos",
                newName: "RuleChannelId");

            migrationBuilder.AddColumn<ulong>(
                name: "LinkMessageId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "OtherChannelId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "AnimeAirChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Premium",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Blacklists",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reason = table.Column<string>(nullable: true),
                    ResponsibleUser = table.Column<ulong>(nullable: false),
                    Unban = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blacklists");

            migrationBuilder.DropColumn(
                name: "LinkMessageId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "OtherChannelId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "AnimeAirChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Premium",
                table: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "RuleChannelId",
                table: "GuildInfos",
                newName: "InviteMessageId");
        }
    }
}
