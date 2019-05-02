using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class LoggingUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LogVoice",
                table: "LoggingConfigs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserColor",
                table: "AccountGlobals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DropConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Emote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Logger = table.Column<string>(nullable: true),
                    CallSite = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DropConfigs");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropColumn(
                name: "LogVoice",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "UserColor",
                table: "AccountGlobals");
        }
    }
}
