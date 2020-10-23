using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class Giveaway : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiveawayHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    IdNum = table.Column<int>(nullable: false),
                    Creator = table.Column<long>(nullable: false),
                    Winner = table.Column<long[]>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    CreatedAtOffset = table.Column<DateTimeOffset>(nullable: false),
                    ClosedAtOffset = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiveawayHistories", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Giveaways",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IdNum = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Creator = table.Column<long>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Stack = table.Column<bool>(nullable: false),
                    WinnerAmount = table.Column<int>(nullable: false),
                    LevelRequirement = table.Column<int>(nullable: true),
                    AccountAgeRequirement = table.Column<TimeSpan>(nullable: true),
                    ServerAgeRequirement = table.Column<TimeSpan>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    CreatedAtOffset = table.Column<DateTimeOffset>(nullable: false),
                    CloseAtOffset = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Giveaways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiveawayParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Entry = table.Column<DateTimeOffset>(nullable: false),
                    GiveawayId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiveawayParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiveawayParticipants_Giveaways_GiveawayId",
                        column: x => x.GiveawayId,
                        principalTable: "Giveaways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiveawayParticipants_GiveawayId",
                table: "GiveawayParticipants",
                column: "GiveawayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiveawayHistories");

            migrationBuilder.DropTable(
                name: "GiveawayParticipants");

            migrationBuilder.DropTable(
                name: "Giveaways");
        }
    }
}
