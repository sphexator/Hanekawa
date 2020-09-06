using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class DblVoteRewards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DblAuths",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    AuthKey = table.Column<Guid>(nullable: false),
                    ExpGain = table.Column<int>(nullable: false),
                    CreditGain = table.Column<int>(nullable: false),
                    SpecialCredit = table.Column<int>(nullable: false),
                    RoleIdReward = table.Column<long>(nullable: true),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DslAuths", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "VoteLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Time = table.Column<DateTimeOffset>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DblAuths");

            migrationBuilder.DropTable(
                name: "VoteLogs");
        }
    }
}
