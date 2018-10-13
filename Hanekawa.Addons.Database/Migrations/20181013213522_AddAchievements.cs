using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Addons.Database.Migrations
{
    public partial class AddAchievements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementDifficulty",
                columns: table => new
                {
                    DifficultyId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementDifficulty", x => x.DifficultyId);
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
                name: "AchievementType",
                columns: table => new
                {
                    TypeId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementType", x => x.TypeId);
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
                    Reward = table.Column<int>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    TypeId = table.Column<int>(nullable: false),
                    DifficultyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementDifficulty_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "AchievementDifficulty",
                        principalColumn: "DifficultyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementType_TypeId",
                        column: x => x.TypeId,
                        principalTable: "AchievementType",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementTrackers");

            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "AchievementDifficulty");

            migrationBuilder.DropTable(
                name: "AchievementType");
        }
    }
}
