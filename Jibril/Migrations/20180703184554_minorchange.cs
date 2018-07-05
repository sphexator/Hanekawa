using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class minorchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "Id",
                table: "Reports",
                nullable: false,
                defaultValue: 0u)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Reports_Id",
                table: "Reports",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Reports_Id",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Reports");
        }
    }
}
