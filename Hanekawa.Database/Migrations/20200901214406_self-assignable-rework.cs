using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class selfassignablerework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmoteMessageFormat",
                table: "SelfAssignAbleRoles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmoteReactFormat",
                table: "SelfAssignAbleRoles",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SelfAssignableChannel",
                table: "ChannelConfigs",
                nullable: true);

            migrationBuilder.AddColumn<long[]>(
                name: "SelfAssignableMessages",
                table: "ChannelConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmoteMessageFormat",
                table: "SelfAssignAbleRoles");

            migrationBuilder.DropColumn(
                name: "EmoteReactFormat",
                table: "SelfAssignAbleRoles");

            migrationBuilder.DropColumn(
                name: "SelfAssignableChannel",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "SelfAssignableMessages",
                table: "ChannelConfigs");
        }
    }
}
