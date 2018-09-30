using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class QnAAddition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "QuestionAndAnswerChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionAndAnswers",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    MessageId = table.Column<ulong>(nullable: true),
                    ResponseUser = table.Column<ulong>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAndAnswers", x => new { x.Id, x.GuildId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAndAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionAndAnswerChannel",
                table: "GuildConfigs");
        }
    }
}
