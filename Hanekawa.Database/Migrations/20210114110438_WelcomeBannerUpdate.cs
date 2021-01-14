using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class WelcomeBannerUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvatarSize",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "AviPlaceX",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "AviPlaceY",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "TextPlaceX",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 245);

            migrationBuilder.AddColumn<int>(
                name: "TextPlaceY",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 40);

            migrationBuilder.AddColumn<int>(
                name: "TextSize",
                table: "WelcomeBanners",
                type: "integer",
                nullable: false,
                defaultValue: 33);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarSize",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "AviPlaceX",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "AviPlaceY",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "TextPlaceX",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "TextPlaceY",
                table: "WelcomeBanners");

            migrationBuilder.DropColumn(
                name: "TextSize",
                table: "WelcomeBanners");
        }
    }
}
