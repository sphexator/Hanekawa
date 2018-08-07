using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Migrations
{
    public partial class botgameUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Consumable",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Unique",
                table: "Inventory");

            migrationBuilder.RenameTable(
                name: "Inventory",
                newName: "Inventories");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "GameEnemies",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MuteTimer",
                table: "Warns",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Elite",
                table: "GameEnemies",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Rare",
                table: "GameEnemies",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(ulong))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Inventories",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<uint>(
                name: "ItemId",
                table: "Inventories",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "UserId", "GuildId" });

            migrationBuilder.CreateTable(
                name: "GameClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ChanceAvoid = table.Column<int>(nullable: false),
                    ChanceCrit = table.Column<int>(nullable: false),
                    ModifierHealth = table.Column<double>(nullable: false),
                    ModifierDamage = table.Column<double>(nullable: false),
                    ModifierAvoidance = table.Column<double>(nullable: false),
                    ModifierCriticalChance = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DefaultHealth = table.Column<int>(nullable: false),
                    DefaultDamage = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryGlobals",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<uint>(nullable: false),
                    Amount = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryGlobals", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameClasses");

            migrationBuilder.DropTable(
                name: "GameConfigs");

            migrationBuilder.DropTable(
                name: "InventoryGlobals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MuteTimer",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "Elite",
                table: "GameEnemies");

            migrationBuilder.DropColumn(
                name: "Rare",
                table: "GameEnemies");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Inventories");

            migrationBuilder.RenameTable(
                name: "Inventories",
                newName: "Inventory");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "GameEnemies",
                newName: "Image");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Inventory",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "Consumable",
                table: "Inventory",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Inventory",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Unique",
                table: "Inventory",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory",
                column: "UserId");
        }
    }
}
