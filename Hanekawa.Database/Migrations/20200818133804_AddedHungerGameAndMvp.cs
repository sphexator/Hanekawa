using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class AddedHungerGameAndMvp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_AchievementDifficulties_DifficultyId",
                table: "Achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_AchievementTypes_TypeId",
                table: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementDifficulties");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_DifficultyId",
                table: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_TypeId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "DifficultyId",
                table: "Achievements");

            migrationBuilder.AddColumn<long>(
                name: "HungerGameChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MvpChannel",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ApprovalQueues",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AchievementDifficulty",
                table: "Achievements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AchievementTypeTypeId",
                table: "Achievements",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MvpCount",
                table: "Accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MvpOptOut",
                table: "Accounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Winner = table.Column<long>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    Start = table.Column<DateTimeOffset>(nullable: false),
                    End = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HgItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Amount = table.Column<int>(nullable: false),
                    GiveOrTake = table.Column<int>(nullable: false),
                    Ammo = table.Column<int>(nullable: false),
                    BleedEffect = table.Column<bool>(nullable: false),
                    HealOverTime = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HgItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HgParticipants",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    Alive = table.Column<bool>(nullable: false),
                    Health = table.Column<int>(nullable: false),
                    Stamina = table.Column<int>(nullable: false),
                    Hunger = table.Column<int>(nullable: false),
                    Thirst = table.Column<int>(nullable: false),
                    Tiredness = table.Column<int>(nullable: false),
                    Bleeding = table.Column<bool>(nullable: false),
                    LatestMove = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HgParticipants", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "MvpConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Day = table.Column<string>(nullable: false),
                    RoleId = table.Column<long>(nullable: true),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MvpConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "HgInventories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HgInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HgInventories_HgItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "HgItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HgInventories_HgParticipants_UserGuildId_UserId",
                        columns: x => new { x.UserGuildId, x.UserId },
                        principalTable: "HgParticipants",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Backgrounds",
                columns: new[] { "Id", "BackgroundUrl" },
                values: new object[,]
                {
                    { 1, "https://i.imgur.com/epIb29P.png" },
                    { 2, "https://i.imgur.com/04PbzvT.png" },
                    { 3, "https://i.imgur.com/5ojmh76.png" },
                    { 4, "https://i.imgur.com/OAMpNDh.png" },
                    { 5, "https://i.imgur.com/KXO5bx5.png" },
                    { 6, "https://i.imgur.com/5h5zZ7C.png" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementTypeTypeId",
                table: "Achievements",
                column: "AchievementTypeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HgInventories_ItemId",
                table: "HgInventories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HgInventories_UserGuildId_UserId",
                table: "HgInventories",
                columns: new[] { "UserGuildId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_AchievementTypes_AchievementTypeTypeId",
                table: "Achievements",
                column: "AchievementTypeTypeId",
                principalTable: "AchievementTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_AchievementTypes_AchievementTypeTypeId",
                table: "Achievements");

            migrationBuilder.DropTable(
                name: "GameHistory");

            migrationBuilder.DropTable(
                name: "HgInventories");

            migrationBuilder.DropTable(
                name: "MvpConfigs");

            migrationBuilder.DropTable(
                name: "HgItems");

            migrationBuilder.DropTable(
                name: "HgParticipants");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_AchievementTypeTypeId",
                table: "Achievements");

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "HungerGameChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MvpChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "AchievementDifficulty",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "AchievementTypeTypeId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "MvpCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MvpOptOut",
                table: "Accounts");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ApprovalQueues",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "DifficultyId",
                table: "Achievements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AchievementDifficulties",
                columns: table => new
                {
                    DifficultyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementDifficulties", x => x.DifficultyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_DifficultyId",
                table: "Achievements",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_TypeId",
                table: "Achievements",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_AchievementDifficulties_DifficultyId",
                table: "Achievements",
                column: "DifficultyId",
                principalTable: "AchievementDifficulties",
                principalColumn: "DifficultyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_AchievementTypes_TypeId",
                table: "Achievements",
                column: "TypeId",
                principalTable: "AchievementTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
