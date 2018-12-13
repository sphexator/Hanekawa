using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class CfgEmbedAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WarnStats",
                table: "WarnStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MuteStats",
                table: "MuteStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageStats",
                table: "MessageStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinStats",
                table: "JoinStats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BanStats",
                table: "BanStats");

            migrationBuilder.RenameTable(
                name: "WarnStats",
                newName: "WarnStat");

            migrationBuilder.RenameTable(
                name: "MuteStats",
                newName: "MuteStat");

            migrationBuilder.RenameTable(
                name: "MessageStats",
                newName: "MessageStat");

            migrationBuilder.RenameTable(
                name: "JoinStats",
                newName: "JoinStat");

            migrationBuilder.RenameTable(
                name: "BanStats",
                newName: "BanStat");

            migrationBuilder.AddColumn<uint>(
                name: "EmbedColor",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarnStat",
                table: "WarnStat",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MuteStat",
                table: "MuteStat",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageStat",
                table: "MessageStat",
                column: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinStat",
                table: "JoinStat",
                column: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BanStat",
                table: "BanStat",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateTable(
                name: "SelfAssignAbleRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    RoleId = table.Column<ulong>(nullable: false),
                    Exclusive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignAbleRoles", x => new { x.GuildId, x.RoleId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelfAssignAbleRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WarnStat",
                table: "WarnStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MuteStat",
                table: "MuteStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageStat",
                table: "MessageStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinStat",
                table: "JoinStat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BanStat",
                table: "BanStat");

            migrationBuilder.DropColumn(
                name: "EmbedColor",
                table: "GuildConfigs");

            migrationBuilder.RenameTable(
                name: "WarnStat",
                newName: "WarnStats");

            migrationBuilder.RenameTable(
                name: "MuteStat",
                newName: "MuteStats");

            migrationBuilder.RenameTable(
                name: "MessageStat",
                newName: "MessageStats");

            migrationBuilder.RenameTable(
                name: "JoinStat",
                newName: "JoinStats");

            migrationBuilder.RenameTable(
                name: "BanStat",
                newName: "BanStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarnStats",
                table: "WarnStats",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MuteStats",
                table: "MuteStats",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageStats",
                table: "MessageStats",
                column: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinStats",
                table: "JoinStats",
                column: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BanStats",
                table: "BanStats",
                columns: new[] { "GuildId", "UserId" });
        }
    }
}
