using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class InitialAddFour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Premium",
                table: "GuildConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reward = table.Column<int>(type: "integer", nullable: true),
                    Requirement = table.Column<int>(type: "integer", nullable: false),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Uploader = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UploadTimeOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalQueues", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BackgroundUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Leader = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: false),
                    AutoAdd = table.Column<bool>(type: "boolean", nullable: false),
                    AdMessage = table.Column<long>(type: "bigint", nullable: true),
                    Channel = table.Column<long>(type: "bigint", nullable: true),
                    Role = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventPayouts",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPayouts", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "GameClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    LevelRequirement = table.Column<int>(type: "integer", nullable: false),
                    ChanceAvoid = table.Column<int>(type: "integer", nullable: false),
                    ChanceCrit = table.Column<int>(type: "integer", nullable: false),
                    ModifierHealth = table.Column<double>(type: "double precision", nullable: false),
                    ModifierDamage = table.Column<double>(type: "double precision", nullable: false),
                    ModifierAvoidance = table.Column<double>(type: "double precision", nullable: false),
                    ModifierCriticalChance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultHealth = table.Column<int>(type: "integer", nullable: false),
                    DefaultDamage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameEnemies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Elite = table.Column<bool>(type: "boolean", nullable: false),
                    Rare = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    ExpGain = table.Column<int>(type: "integer", nullable: false),
                    CreditGain = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEnemies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Highlights",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Keywords = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "HungerGameCustomChars",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameCustomChars", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "HungerGameDefaults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameDefaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameHistories",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Winner = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameHistories", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameProfiles",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Bot = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Health = table.Column<double>(type: "double precision", nullable: false),
                    Stamina = table.Column<double>(type: "double precision", nullable: false),
                    Bleeding = table.Column<bool>(type: "boolean", nullable: false),
                    Hunger = table.Column<double>(type: "double precision", nullable: false),
                    Thirst = table.Column<double>(type: "double precision", nullable: false),
                    Tiredness = table.Column<double>(type: "double precision", nullable: false),
                    Move = table.Column<int>(type: "integer", nullable: false),
                    Food = table.Column<int>(type: "integer", nullable: false),
                    Water = table.Column<int>(type: "integer", nullable: false),
                    FirstAid = table.Column<int>(type: "integer", nullable: false),
                    Weapons = table.Column<int>(type: "integer", nullable: false),
                    MeleeWeapon = table.Column<int>(type: "integer", nullable: false),
                    RangeWeapon = table.Column<int>(type: "integer", nullable: false),
                    Bullets = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameProfiles", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "HungerGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    Alive = table.Column<int>(type: "integer", nullable: false),
                    Participants = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameStatus",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    SignUpChannel = table.Column<long>(type: "bigint", nullable: true),
                    EventChannel = table.Column<long>(type: "bigint", nullable: true),
                    EmoteMessageFormat = table.Column<string>(type: "text", nullable: true),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    SignUpStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SignUpMessage = table.Column<string>(type: "text", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false),
                    RoleReward = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameStatus", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => new { x.GuildId, x.UserId, x.ItemId });
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Sell = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    Role = table.Column<long>(type: "bigint", nullable: true),
                    HealthIncrease = table.Column<int>(type: "integer", nullable: false),
                    DamageIncrease = table.Column<int>(type: "integer", nullable: false),
                    CriticalIncrease = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    DateAdded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LevelExpEvents",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<long>(type: "bigint", nullable: true),
                    MessageId = table.Column<long>(type: "bigint", nullable: true),
                    Multiplier = table.Column<double>(type: "double precision", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpEvents", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ServerStores",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    SpecialCredit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStores", x => new { x.GuildId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "AchievementUnlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccountUserId = table.Column<long>(type: "bigint", nullable: true),
                    AchieveId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementUnlocks_AccountGlobals_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "AccountGlobals",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementUnlocks_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubBlacklists",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    User = table.Column<long>(type: "bigint", nullable: false),
                    Issuer = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubBlacklists", x => new { x.GuildId, x.User });
                    table.ForeignKey(
                        name: "FK_ClubBlacklists_ClubInfos_ClubId",
                        column: x => x.ClubId,
                        principalTable: "ClubInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClubPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubPlayers_ClubInfos_ClubId",
                        column: x => x.ClubId,
                        principalTable: "ClubInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "AchievementId", "Category", "Description", "Difficulty", "Hidden", "ImageUrl", "Name", "Points", "Requirement", "Reward" },
                values: new object[,]
                {
                    { new Guid("35c0c55e-1624-496e-80e3-6b749bce7ec9"), 0, "Reach Server Level 5", 0, false, "", "Level 5", 10, 5, null },
                    { new Guid("cf99b550-9f40-4fae-af4f-d752c623764a"), 0, "Reach Server Level 10", 0, false, "", "Level 10", 10, 10, null },
                    { new Guid("52394c8d-f480-415e-94b8-705cd5785c41"), 0, "Reach Server Level 20", 0, false, "", "Level 20", 10, 20, null },
                    { new Guid("9a8ab7a7-d522-402f-af50-92fa9abad84f"), 0, "Reach Server Level 30", 0, false, "", "Level 30", 10, 30, null },
                    { new Guid("fd0261d9-5590-4dfd-ac44-d5b4ecd7f249"), 0, "Reach Server Level 40", 0, false, "", "Level 40", 10, 40, null },
                    { new Guid("4bd811cc-f752-4c3c-85a3-ed49e410f88e"), 0, "Reach Server Level 50", 1, false, "", "Level 50", 10, 50, null },
                    { new Guid("c909b440-8777-4dc7-82e8-ea0f0255b3a3"), 0, "Reach Server Level 60", 1, false, "", "Level 60", 10, 60, null },
                    { new Guid("44f7cfc4-4d23-44f8-8d21-09b5daad169d"), 0, "Reach Server Level 70", 2, false, "", "Reach Server Level 70", 10, 70, null },
                    { new Guid("c8612766-f919-4d35-abaa-10078baa7da3"), 0, "Reach Server Level 80", 2, false, "", "Level 80", 10, 80, null },
                    { new Guid("f67f05d9-6424-4eeb-b5e8-c16811cbea64"), 0, "Reach Server Level 90", 3, false, "", "Level 90", 10, 90, null },
                    { new Guid("bb923ced-e792-4c72-aff0-667beb7f0b3d"), 0, "Reach Server Level 100", 3, false, "", "Level 100", 10, 100, null }
                });

            migrationBuilder.InsertData(
                table: "Backgrounds",
                columns: new[] { "Id", "BackgroundUrl" },
                values: new object[,]
                {
                    { new Guid("95d9c246-6def-46f0-a8f7-f749a92b4c33"), "https://i.imgur.com/OAMpNDh.png" },
                    { new Guid("2787c44e-acfb-444a-ac38-beb72ad44919"), "https://i.imgur.com/5ojmh76.png" },
                    { new Guid("be1dc76c-23a9-4284-be37-dee68f957129"), "https://i.imgur.com/04PbzvT.png" },
                    { new Guid("26ca7657-c60f-4fe0-9f15-807fcd506e43"), "https://i.imgur.com/epIb29P.png" },
                    { new Guid("16924135-5103-42ed-8ce3-fca4a61bb179"), "https://i.imgur.com/KXO5bx5.png" },
                    { new Guid("39d2e876-de8d-4e76-a004-c373a0ffbb3e"), "https://i.imgur.com/5h5zZ7C.png" }
                });

            migrationBuilder.InsertData(
                table: "HungerGameDefaults",
                columns: new[] { "Id", "Avatar", "Name" },
                values: new object[,]
                {
                    { 7L, "https://i.imgur.com/VLsezdF.png", "Akagi" },
                    { 1L, "https://i.imgur.com/XMjW8Qn.png", "Dia" },
                    { 2L, "https://i.imgur.com/7URjbvT.png", "Kanan" },
                    { 3L, "https://i.imgur.com/tPDON9P.png", "Yoshiko" },
                    { 4L, "https://i.imgur.com/dcB1loo.png", "Kongou" },
                    { 25L, "https://i.imgur.com/Wxhd5WY.png", "Shiro" },
                    { 24L, "https://i.imgur.com/aijxHla.png", "Vanilla" },
                    { 23L, "https://i.imgur.com/HoNwKi9.png", "Chocola" },
                    { 22L, "https://i.imgur.com/bv5ao8Z.png", "Enterprise" },
                    { 21L, "https://i.imgur.com/VyJf95i.png", "Bocchi" },
                    { 20L, "https://i.imgur.com/GhSG97V.png", "Shiina" },
                    { 6L, "https://i.imgur.com/8748bUL.png", "Yamato" },
                    { 19L, "https://i.imgur.com/CI9Osi5.png", "Akame" },
                    { 17L, "https://i.imgur.com/5xR0ImK.png", "Sora" },
                    { 16L, "https://i.imgur.com/PT8SsVB.png", "Chika" },
                    { 15L, "https://i.imgur.com/rYa5iYc.png", "Shiki" },
                    { 14L, "https://i.imgur.com/0VYBYEg.png", "Gura" },
                    { 13L, "https://i.imgur.com/5CcdVBE.png", "Ram" },
                    { 12L, "https://i.imgur.com/y3bb8Sk.png", "Rem" },
                    { 11L, "https://i.imgur.com/kF9b4SJ.png", "Emilia" },
                    { 5L, "https://i.imgur.com/7GC7FvJ.png", "Haruna" },
                    { 9L, "https://i.imgur.com/4XYg6ch.png", "Zero Two" },
                    { 8L, "https://i.imgur.com/eyt9k8E.png", "Kaga" },
                    { 18L, "https://i.imgur.com/U0NlfJd.png", "Nobuna" },
                    { 10L, "https://i.imgur.com/Nl6WsbP.png", "Echidna" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementUnlocks_AccountUserId",
                table: "AchievementUnlocks",
                column: "AccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementUnlocks_AchievementId",
                table: "AchievementUnlocks",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubBlacklists_ClubId",
                table: "ClubBlacklists",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubPlayers_ClubId",
                table: "ClubPlayers",
                column: "ClubId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "ApprovalQueues");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "ClubBlacklists");

            migrationBuilder.DropTable(
                name: "ClubPlayers");

            migrationBuilder.DropTable(
                name: "EventPayouts");

            migrationBuilder.DropTable(
                name: "GameClasses");

            migrationBuilder.DropTable(
                name: "GameConfigs");

            migrationBuilder.DropTable(
                name: "GameEnemies");

            migrationBuilder.DropTable(
                name: "Highlights");

            migrationBuilder.DropTable(
                name: "HungerGameCustomChars");

            migrationBuilder.DropTable(
                name: "HungerGameDefaults");

            migrationBuilder.DropTable(
                name: "HungerGameHistories");

            migrationBuilder.DropTable(
                name: "HungerGameProfiles");

            migrationBuilder.DropTable(
                name: "HungerGames");

            migrationBuilder.DropTable(
                name: "HungerGameStatus");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "LevelExpEvents");

            migrationBuilder.DropTable(
                name: "ServerStores");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "ClubInfos");

            migrationBuilder.DropColumn(
                name: "Premium",
                table: "GuildConfigs");
        }
    }
}
