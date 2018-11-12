using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class AddedPatreon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Tolerance",
                table: "SingleNudeServiceChannels",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "SingleNudeServiceChannels",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "InHouse",
                table: "SingleNudeServiceChannels",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InHouse",
                table: "NudeServiceChannels",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Patreons",
                columns: table => new
                {
                    BotId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Added = table.Column<DateTime>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Rewarded = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patreons", x => new { x.BotId, x.UserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patreons");

            migrationBuilder.DropColumn(
                name: "InHouse",
                table: "SingleNudeServiceChannels");

            migrationBuilder.DropColumn(
                name: "InHouse",
                table: "NudeServiceChannels");

            migrationBuilder.AlterColumn<int>(
                name: "Tolerance",
                table: "SingleNudeServiceChannels",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "SingleNudeServiceChannels",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
