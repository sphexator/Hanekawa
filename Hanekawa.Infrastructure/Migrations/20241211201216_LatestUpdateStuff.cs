using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hanekawa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LatestUpdateStuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_User_UserId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserId",
                table: "Users",
                newName: "IX_Users_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_User_Id",
                table: "Users",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_User_Id",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Id",
                table: "Users",
                newName: "IX_Users_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_User_UserId",
                table: "Users",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
