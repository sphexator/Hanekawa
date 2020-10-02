using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class HungerGames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameHistory");

            migrationBuilder.DropTable(
                name: "HgInventories");

            migrationBuilder.DropTable(
                name: "HgItems");

            migrationBuilder.DropTable(
                name: "HgParticipants");

            migrationBuilder.CreateTable(
                name: "HungerGameCustomChars",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameCustomChars", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "HungerGameDefaults",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameDefaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameHistories",
                columns: table => new
                {
                    GameId = table.Column<Guid>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Winner = table.Column<long>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    ExpReward = table.Column<int>(nullable: false),
                    CreditReward = table.Column<int>(nullable: false),
                    SpecialCreditReward = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameHistories", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameProfiles",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Bot = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    Alive = table.Column<bool>(nullable: false),
                    Health = table.Column<double>(nullable: false),
                    Stamina = table.Column<double>(nullable: false),
                    Bleeding = table.Column<bool>(nullable: false),
                    Hunger = table.Column<double>(nullable: false),
                    Thirst = table.Column<double>(nullable: false),
                    Tiredness = table.Column<double>(nullable: false),
                    Move = table.Column<int>(nullable: false),
                    Food = table.Column<int>(nullable: false),
                    Water = table.Column<int>(nullable: false),
                    FirstAid = table.Column<int>(nullable: false),
                    Weapons = table.Column<int>(nullable: false),
                    MeleeWeapon = table.Column<int>(nullable: false),
                    RangeWeapon = table.Column<int>(nullable: false),
                    Bullets = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameProfiles", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "HungerGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Round = table.Column<int>(nullable: false),
                    Alive = table.Column<int>(nullable: false),
                    Participants = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameStatus",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    SignUpChannel = table.Column<long>(nullable: true),
                    EventChannel = table.Column<long>(nullable: true),
                    EmoteMessageFormat = table.Column<string>(nullable: true),
                    Stage = table.Column<int>(nullable: false),
                    SignUpStart = table.Column<DateTimeOffset>(nullable: false),
                    SignUpMessage = table.Column<string>(nullable: true),
                    GameId = table.Column<Guid>(nullable: true),
                    ExpReward = table.Column<int>(nullable: false),
                    CreditReward = table.Column<int>(nullable: false),
                    SpecialCreditReward = table.Column<int>(nullable: false),
                    RoleReward = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameStatus", x => x.GuildId);
                });

            migrationBuilder.InsertData(
                table: "HungerGameDefaults",
                columns: new[] { "Id", "Avatar", "Name" },
                values: new object[,]
                {
                    { 1L, "https://i.imgur.com/XMjW8Qn.png", "Dia" },
                    { 23L, "https://i.imgur.com/HoNwKi9.png", "Chocola" },
                    { 22L, "https://i.imgur.com/bv5ao8Z.png", "Enterprise" },
                    { 21L, "https://i.imgur.com/VyJf95i.png", "Bocchi" },
                    { 20L, "https://i.imgur.com/GhSG97V.png", "Shiina" },
                    { 19L, "https://i.imgur.com/CI9Osi5.png", "Akame" },
                    { 18L, "https://i.imgur.com/U0NlfJd.png", "Nobuna" },
                    { 17L, "https://i.imgur.com/5xR0ImK.png", "Sora" },
                    { 16L, "https://i.imgur.com/PT8SsVB.png", "Chika" },
                    { 15L, "https://i.imgur.com/rYa5iYc.png", "Shiki" },
                    { 14L, "https://i.imgur.com/0VYBYEg.png", "Gura" },
                    { 24L, "https://i.imgur.com/aijxHla.png", "Vanilla" },
                    { 13L, "https://i.imgur.com/5CcdVBE.png", "Ram" },
                    { 11L, "https://i.imgur.com/kF9b4SJ.png", "Emilia" },
                    { 10L, "https://i.imgur.com/Nl6WsbP.png", "Echidna" },
                    { 9L, "https://i.imgur.com/4XYg6ch.png", "Zero Two" },
                    { 8L, "https://i.imgur.com/eyt9k8E.png", "Kaga" },
                    { 7L, "https://i.imgur.com/VLsezdF.png", "Akagi" },
                    { 6L, "https://i.imgur.com/8748bUL.png", "Yamato" },
                    { 5L, "https://i.imgur.com/7GC7FvJ.png", "Haruna" },
                    { 4L, "https://i.imgur.com/dcB1loo.png", "Kongou" },
                    { 3L, "https://i.imgur.com/tPDON9P.png", "Yoshiko" },
                    { 2L, "https://i.imgur.com/7URjbvT.png", "Kanan" },
                    { 12L, "https://i.imgur.com/y3bb8Sk.png", "Rem" },
                    { 25L, "https://i.imgur.com/Wxhd5WY.png", "Shiro" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "GameHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Winner = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HgItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ammo = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    BleedEffect = table.Column<bool>(type: "boolean", nullable: false),
                    GiveOrTake = table.Column<int>(type: "integer", nullable: false),
                    HealOverTime = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HgItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HgParticipants",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Bleeding = table.Column<bool>(type: "boolean", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Hunger = table.Column<int>(type: "integer", nullable: false),
                    LatestMove = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Stamina = table.Column<int>(type: "integer", nullable: false),
                    Thirst = table.Column<int>(type: "integer", nullable: false),
                    Tiredness = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HgParticipants", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "HgInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    UserGuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_HgInventories_ItemId",
                table: "HgInventories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HgInventories_UserGuildId_UserId",
                table: "HgInventories",
                columns: new[] { "UserGuildId", "UserId" });
        }
    }
}
