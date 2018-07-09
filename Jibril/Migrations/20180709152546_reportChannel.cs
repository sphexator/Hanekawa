using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class reportChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Suggestions",
                table: "Suggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.AlterColumn<ulong>(
                name: "ResponseUser",
                table: "Suggestions",
                nullable: true,
                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Suggestions",
                nullable: false,
                oldClrType: typeof(uint))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Suggestions",
                nullable: true,
                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Reports",
                nullable: true,
                oldClrType: typeof(ulong));

            migrationBuilder.AddColumn<ulong>(
                name: "ReportChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "SuggestionChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suggestions",
                table: "Suggestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Suggestions",
                table: "Suggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "SuggestionChannel",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<ulong>(
                name: "ResponseUser",
                table: "Suggestions",
                nullable: false,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Suggestions",
                nullable: false,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Suggestions",
                nullable: false,
                oldClrType: typeof(uint))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Reports",
                nullable: false,
                oldClrType: typeof(ulong),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suggestions",
                table: "Suggestions",
                columns: new[] { "UserId", "MessageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                columns: new[] { "Id", "MessageId", "UserId" });
        }
    }
}
