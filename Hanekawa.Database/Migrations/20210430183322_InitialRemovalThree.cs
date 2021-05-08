using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class InitialRemovalThree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementTrackers");

            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "ApprovalQueues");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "ClubBlacklists");

            migrationBuilder.DropTable(
                name: "ClubInfos");

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
                name: "MusicConfigs");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "ServerStores");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementNames");

            migrationBuilder.DropTable(
                name: "AchievementTypes");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "LevelExpReductions");

            migrationBuilder.DropColumn(
                name: "AnimeAirChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "AutomaticEventSchedule",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "HungerGameChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MusicChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "MusicVcChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Premium",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "SelfAssignableMessages",
                table: "ChannelConfigs");

            migrationBuilder.AddColumn<long>(
                name: "EmoteId",
                table: "SelfAssignAbleRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogVoice",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong[]>(
                name: "Winner",
                table: "GiveawayHistories",
                type: "numeric[]",
                nullable: true,
                oldClrType: typeof(long[]),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CurrencySignId",
                table: "CurrencyConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SpecialCurrencySignId",
                table: "CurrencyConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTextChannel",
                table: "ClubConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "DropClaims",
                table: "Accounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "GamePvPAmount",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Added = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<long>(type: "bigint", nullable: false),
                    Triggers = table.Column<List<string>>(type: "text[]", nullable: true),
                    LevelCap = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => new { x.GuildId, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignReactionRoles",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Reactions = table.Column<List<string>>(type: "text[]", nullable: true),
                    Exclusive = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignReactionRoles", x => new { x.GuildId, x.ChannelId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_SelfAssignReactionRoles_ChannelConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "ChannelConfigs",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignReactionRoles_ConfigId",
                table: "SelfAssignReactionRoles",
                column: "ConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "SelfAssignReactionRoles");

            migrationBuilder.DropColumn(
                name: "EmoteId",
                table: "SelfAssignAbleRoles");

            migrationBuilder.DropColumn(
                name: "CurrencySignId",
                table: "CurrencyConfigs");

            migrationBuilder.DropColumn(
                name: "SpecialCurrencySignId",
                table: "CurrencyConfigs");

            migrationBuilder.DropColumn(
                name: "EnableTextChannel",
                table: "ClubConfigs");

            migrationBuilder.DropColumn(
                name: "DropClaims",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GamePvPAmount",
                table: "Accounts");

            migrationBuilder.AlterColumn<decimal>(
                name: "LogVoice",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "LevelExpReductions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "AnimeAirChannel",
                table: "GuildConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutomaticEventSchedule",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "HungerGameChannel",
                table: "GuildConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MusicChannel",
                table: "GuildConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MusicVcChannel",
                table: "GuildConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Premium",
                table: "GuildConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long[]>(
                name: "Winner",
                table: "GiveawayHistories",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(ulong[]),
                oldType: "numeric[]",
                oldNullable: true);

            migrationBuilder.AddColumn<long[]>(
                name: "SelfAssignableMessages",
                table: "ChannelConfigs",
                type: "bigint[]",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AchievementNames",
                columns: table => new
                {
                    AchievementNameId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Stackable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementNames", x => x.AchievementNameId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementTrackers",
                columns: table => new
                {
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTrackers", x => new { x.Type, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "AchievementTypes",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTypes", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UploadTimeOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Uploader = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalQueues", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BackgroundUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubBlacklists",
                columns: table => new
                {
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    BlackListUser = table.Column<long>(type: "bigint", nullable: false),
                    IssuedUser = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubBlacklists", x => new { x.ClubId, x.GuildId, x.BlackListUser });
                });

            migrationBuilder.CreateTable(
                name: "ClubInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdMessage = table.Column<long>(type: "bigint", nullable: true),
                    AutoAdd = table.Column<bool>(type: "boolean", nullable: false),
                    Channel = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    LeaderId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: false),
                    Role = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    JoinDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPlayers", x => x.Id);
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChanceAvoid = table.Column<int>(type: "integer", nullable: false),
                    ChanceCrit = table.Column<int>(type: "integer", nullable: false),
                    LevelRequirement = table.Column<long>(type: "bigint", nullable: false),
                    ModifierAvoidance = table.Column<double>(type: "double precision", nullable: false),
                    ModifierCriticalChance = table.Column<double>(type: "double precision", nullable: false),
                    ModifierDamage = table.Column<double>(type: "double precision", nullable: false),
                    ModifierHealth = table.Column<double>(type: "double precision", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DefaultDamage = table.Column<int>(type: "integer", nullable: false),
                    DefaultHealth = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameEnemies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    CreditGain = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    Elite = table.Column<bool>(type: "boolean", nullable: false),
                    ExpGain = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Rare = table.Column<bool>(type: "boolean", nullable: false)
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
                    Highlights = table.Column<string[]>(type: "text[]", nullable: true)
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
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
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
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
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
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false),
                    Winner = table.Column<long>(type: "bigint", nullable: false)
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
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Bleeding = table.Column<bool>(type: "boolean", nullable: false),
                    Bot = table.Column<bool>(type: "boolean", nullable: false),
                    Bullets = table.Column<int>(type: "integer", nullable: false),
                    FirstAid = table.Column<int>(type: "integer", nullable: false),
                    Food = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<double>(type: "double precision", nullable: false),
                    Hunger = table.Column<double>(type: "double precision", nullable: false),
                    MeleeWeapon = table.Column<int>(type: "integer", nullable: false),
                    Move = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RangeWeapon = table.Column<int>(type: "integer", nullable: false),
                    Stamina = table.Column<double>(type: "double precision", nullable: false),
                    Thirst = table.Column<double>(type: "double precision", nullable: false),
                    Tiredness = table.Column<double>(type: "double precision", nullable: false),
                    Water = table.Column<int>(type: "integer", nullable: false),
                    Weapons = table.Column<int>(type: "integer", nullable: false)
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
                    Alive = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Participants = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false)
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
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    EmoteMessageFormat = table.Column<string>(type: "text", nullable: true),
                    EventChannel = table.Column<long>(type: "bigint", nullable: true),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleReward = table.Column<long>(type: "bigint", nullable: true),
                    SignUpChannel = table.Column<long>(type: "bigint", nullable: true),
                    SignUpMessage = table.Column<string>(type: "text", nullable: true),
                    SignUpStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false)
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
                    ItemId = table.Column<int>(type: "integer", nullable: false),
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriticalIncrease = table.Column<int>(type: "integer", nullable: false),
                    DamageIncrease = table.Column<int>(type: "integer", nullable: false),
                    DateAdded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    HealthIncrease = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<long>(type: "bigint", nullable: true),
                    Sell = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
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
                name: "MusicConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    TextChId = table.Column<long>(type: "bigint", nullable: true),
                    VoiceChId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Songs = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => new { x.GuildId, x.Name });
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
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AchievementDifficulty = table.Column<int>(type: "integer", nullable: false),
                    AchievementNameId = table.Column<int>(type: "integer", nullable: false),
                    AchievementTypeTypeId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Global = table.Column<bool>(type: "boolean", nullable: false),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Once = table.Column<bool>(type: "boolean", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Requirement = table.Column<int>(type: "integer", nullable: false),
                    Reward = table.Column<int>(type: "integer", nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_Achievements_AchievementTypes_AchievementTypeTypeId",
                        column: x => x.AchievementTypeTypeId,
                        principalTable: "AchievementTypes",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementUnlocks",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Backgrounds",
                columns: new[] { "Id", "BackgroundUrl" },
                values: new object[,]
                {
                    { 6, "https://i.imgur.com/5h5zZ7C.png" },
                    { 4, "https://i.imgur.com/OAMpNDh.png" },
                    { 3, "https://i.imgur.com/5ojmh76.png" },
                    { 2, "https://i.imgur.com/04PbzvT.png" },
                    { 1, "https://i.imgur.com/epIb29P.png" },
                    { 5, "https://i.imgur.com/KXO5bx5.png" }
                });

            migrationBuilder.InsertData(
                table: "HungerGameDefaults",
                columns: new[] { "Id", "Avatar", "Name" },
                values: new object[,]
                {
                    { 25L, "https://i.imgur.com/Wxhd5WY.png", "Shiro" },
                    { 24L, "https://i.imgur.com/aijxHla.png", "Vanilla" },
                    { 23L, "https://i.imgur.com/HoNwKi9.png", "Chocola" },
                    { 22L, "https://i.imgur.com/bv5ao8Z.png", "Enterprise" },
                    { 21L, "https://i.imgur.com/VyJf95i.png", "Bocchi" },
                    { 20L, "https://i.imgur.com/GhSG97V.png", "Shiina" },
                    { 19L, "https://i.imgur.com/CI9Osi5.png", "Akame" },
                    { 18L, "https://i.imgur.com/U0NlfJd.png", "Nobuna" },
                    { 17L, "https://i.imgur.com/5xR0ImK.png", "Sora" },
                    { 16L, "https://i.imgur.com/PT8SsVB.png", "Chika" },
                    { 14L, "https://i.imgur.com/0VYBYEg.png", "Gura" },
                    { 13L, "https://i.imgur.com/5CcdVBE.png", "Ram" },
                    { 12L, "https://i.imgur.com/y3bb8Sk.png", "Rem" },
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
                    { 15L, "https://i.imgur.com/rYa5iYc.png", "Shiki" },
                    { 1L, "https://i.imgur.com/XMjW8Qn.png", "Dia" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementNameId",
                table: "Achievements",
                column: "AchievementNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AchievementTypeTypeId",
                table: "Achievements",
                column: "AchievementTypeTypeId");
        }
    }
}
