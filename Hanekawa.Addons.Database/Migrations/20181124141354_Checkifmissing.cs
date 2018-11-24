using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class Checkifmissing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildInfoLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildInfos",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "FaqOne",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "FaqOneMessageId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "FaqTwo",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "FaqTwoMessageId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "LevelMessageId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "LinkMessageId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "OtherChannelId",
                table: "GuildInfos");

            migrationBuilder.DropColumn(
                name: "RuleChannelId",
                table: "GuildInfos");

            migrationBuilder.RenameColumn(
                name: "StaffMessageId",
                table: "GuildInfos",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "Rules",
                table: "GuildInfos",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "RuleMessageId",
                table: "GuildInfos",
                newName: "ChannelId");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GuildInfos",
                nullable: false,
                oldClrType: typeof(ulong))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildInfos",
                table: "GuildInfos",
                columns: new[] { "GuildId", "ChannelId", "MessageId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildInfos",
                table: "GuildInfos");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "GuildInfos",
                newName: "Rules");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "GuildInfos",
                newName: "StaffMessageId");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "GuildInfos",
                newName: "RuleMessageId");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GuildInfos",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "FaqOne",
                table: "GuildInfos",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "FaqOneMessageId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "FaqTwo",
                table: "GuildInfos",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "FaqTwoMessageId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "LevelMessageId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

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
                name: "RuleChannelId",
                table: "GuildInfos",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildInfos",
                table: "GuildInfos",
                column: "GuildId");

            migrationBuilder.CreateTable(
                name: "GuildInfoLinks",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Link = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInfoLinks", x => x.GuildId);
                });
        }
    }
}
