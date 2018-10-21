using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class UpdateAchievement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shops",
                table: "Shops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryGlobals",
                table: "InventoryGlobals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Item",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ROle",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Shops");

            migrationBuilder.RenameColumn(
                name: "StackAble",
                table: "Items",
                newName: "Secret");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Shops",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Shops",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Shops",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SpecialCredit",
                table: "Shops",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsumeOnUse",
                table: "Items",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "Items",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Global",
                table: "Items",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "Role",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecretValue",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Items",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(ulong))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shops",
                table: "Shops",
                columns: new[] { "GuildId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryGlobals",
                table: "InventoryGlobals",
                columns: new[] { "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "GuildId", "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateTable(
                name: "AchievementDifficulties",
                columns: table => new
                {
                    DifficultyId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementDifficulties", x => x.DifficultyId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementNames",
                columns: table => new
                {
                    AchievementNameId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Stackable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementNames", x => x.AchievementNameId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementTrackers",
                columns: table => new
                {
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTrackers", x => new { x.Type, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "AchievementTypes",
                columns: table => new
                {
                    TypeId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTypes", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "StoreGlobals",
                columns: table => new
                {
                    ItemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreGlobals", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Requirement = table.Column<int>(nullable: false),
                    Once = table.Column<bool>(nullable: false),
                    Reward = table.Column<int>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: false),
                    Global = table.Column<bool>(nullable: false),
                    AchievementNameId = table.Column<int>(nullable: false),
                    TypeId = table.Column<int>(nullable: false),
                    DifficultyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementNames_AchievementNameId",
                        column: x => x.AchievementNameId,
                        principalTable: "AchievementNames",
                        principalColumn: "AchievementNameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementDifficulties_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "AchievementDifficulties",
                        principalColumn: "DifficultyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "AchievementTypes",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AchievementUnlocks",
                columns: table => new
                {
                    AchievementId = table.Column<int>(nullable: false),
                    TypeId = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementUnlocks", x => new { x.AchievementId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AchievementUnlocks_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementNameId",
                table: "Achievements",
                column: "AchievementNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_DifficultyId",
                table: "Achievements",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_TypeId",
                table: "Achievements",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementTrackers");

            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "StoreGlobals");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementNames");

            migrationBuilder.DropTable(
                name: "AchievementDifficulties");

            migrationBuilder.DropTable(
                name: "AchievementTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shops",
                table: "Shops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryGlobals",
                table: "InventoryGlobals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SpecialCredit",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ConsumeOnUse",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Global",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SecretValue",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Secret",
                table: "Items",
                newName: "StackAble");

            migrationBuilder.AlterColumn<uint>(
                name: "Price",
                table: "Shops",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<uint>(
                name: "Id",
                table: "Shops",
                nullable: false,
                defaultValue: 0u)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "Item",
                table: "Shops",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ROle",
                table: "Shops",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "RoleId",
                table: "Shops",
                nullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "Amount",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "ItemId",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "InventoryGlobals",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "Amount",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<uint>(
                name: "ItemId",
                table: "Inventories",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shops",
                table: "Shops",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryGlobals",
                table: "InventoryGlobals",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                columns: new[] { "UserId", "GuildId" });

            migrationBuilder.CreateTable(
                name: "ShopEvents",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Item = table.Column<string>(nullable: true),
                    Price = table.Column<uint>(nullable: false),
                    Stock = table.Column<uint>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopEvents", x => x.Id);
                });
        }
    }
}
