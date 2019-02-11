using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class clubUpdate : Migration
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
                name: "Leader",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ClubInfos");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelExpEvents",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<bool>(
                name: "ClubRole",
                table: "GuildConfigs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "LeaderId",
                table: "ClubInfos",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Role",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Blacklists",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AccountGlobals",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "ClubRole",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ClubInfos");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelExpEvents",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigs",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<long>(
                name: "Leader",
                table: "ClubInfos",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "ClubInfos",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Blacklists",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AccountGlobals",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubPlayers",
                table: "ClubPlayers",
                columns: new[] { "Id", "ClubId", "GuildId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClubInfos",
                table: "ClubInfos",
                columns: new[] { "Id", "GuildId", "Leader" });
        }
    }
}
