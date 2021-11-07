using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGlobals",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Exp = table.Column<int>(type: "integer", nullable: false),
                    TotalExp = table.Column<int>(type: "integer", nullable: false),
                    Rep = table.Column<int>(type: "integer", nullable: false),
                    Credit = table.Column<int>(type: "integer", nullable: false),
                    StarReceive = table.Column<int>(type: "integer", nullable: false),
                    StarGive = table.Column<int>(type: "integer", nullable: false),
                    UserColor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGlobals", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Credit = table.Column<int>(type: "integer", nullable: false),
                    CreditSpecial = table.Column<int>(type: "integer", nullable: false),
                    DailyCredit = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Exp = table.Column<int>(type: "integer", nullable: false),
                    TotalExp = table.Column<int>(type: "integer", nullable: false),
                    Decay = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VoiceExpTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Class = table.Column<int>(type: "integer", nullable: false),
                    ProfilePic = table.Column<string>(type: "text", nullable: true),
                    Rep = table.Column<int>(type: "integer", nullable: false),
                    RepCooldown = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GameKillAmount = table.Column<int>(type: "integer", nullable: false),
                    GamePvPAmount = table.Column<int>(type: "integer", nullable: false),
                    FirstMessage = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastMessage = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatVoiceTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Sessions = table.Column<int>(type: "integer", nullable: false),
                    StatMessages = table.Column<long>(type: "bigint", nullable: false),
                    DropClaims = table.Column<long>(type: "bigint", nullable: false),
                    StarGiven = table.Column<int>(type: "integer", nullable: false),
                    StarReceived = table.Column<int>(type: "integer", nullable: false),
                    ChannelVoiceTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MvpCount = table.Column<int>(type: "integer", nullable: false),
                    MvpOptOut = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => new { x.GuildId, x.UserId });
                });

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
                name: "AdminConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    MuteRole = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    FilterInvites = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreAllChannels = table.Column<bool>(type: "boolean", nullable: false),
                    FilterMsgLength = table.Column<int>(type: "integer", nullable: true),
                    FilterUrls = table.Column<bool>(type: "boolean", nullable: false),
                    FilterAllInv = table.Column<bool>(type: "boolean", nullable: false),
                    EmoteCountFilter = table.Column<int>(type: "integer", nullable: true),
                    MentionCountFilter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Uploader = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UploadTimeOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalQueues", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "AutoMessages",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Creator = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Webhook = table.Column<string>(type: "text", nullable: true),
                    WebhookId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Interval = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoMessages", x => new { x.GuildId, x.Name });
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
                name: "Blacklists",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ResponsibleUser = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Unban = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "BoardConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Emote = table.Column<string>(type: "text", nullable: true),
                    Channel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Webhook = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    StarAmount = table.Column<int>(type: "integer", nullable: false),
                    Boarded = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => new { x.GuildId, x.MessageId });
                });

            migrationBuilder.CreateTable(
                name: "BoostConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Webhook = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreditGain = table.Column<int>(type: "integer", nullable: false),
                    SpecialCreditGain = table.Column<int>(type: "integer", nullable: false),
                    ExpGain = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ReportChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookReport = table.Column<string>(type: "text", nullable: true),
                    SelfAssignableChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ClubConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelCategory = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    AdvertisementChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    EnableTextChannel = table.Column<bool>(type: "boolean", nullable: false),
                    EnableVoiceChannel = table.Column<bool>(type: "boolean", nullable: false),
                    ChannelRequiredAmount = table.Column<int>(type: "integer", nullable: false),
                    ChannelRequiredLevel = table.Column<int>(type: "integer", nullable: false),
                    AutoPrune = table.Column<bool>(type: "boolean", nullable: false),
                    RoleEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ClubInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Leader = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: false),
                    AutoAdd = table.Column<bool>(type: "boolean", nullable: false),
                    AdMessage = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Channel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Role = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    EmoteCurrency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CurrencyName = table.Column<string>(type: "text", nullable: true),
                    CurrencySign = table.Column<string>(type: "text", nullable: true),
                    CurrencySignId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    SpecialEmoteCurrency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SpecialCurrencyName = table.Column<string>(type: "text", nullable: true),
                    SpecialCurrencySign = table.Column<string>(type: "text", nullable: true),
                    SpecialCurrencySignId = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "DblAuths",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    AuthKey = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpGain = table.Column<int>(type: "integer", nullable: false),
                    CreditGain = table.Column<int>(type: "integer", nullable: false),
                    SpecialCredit = table.Column<int>(type: "integer", nullable: false),
                    RoleIdReward = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DblAuths", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "DropChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "DropConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Emote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropConfigs", x => x.GuildId);
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
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                name: "GiveawayHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    IdNum = table.Column<int>(type: "integer", nullable: false),
                    Creator = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Winner = table.Column<ulong[]>(type: "numeric[]", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAtOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiveawayHistories", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Giveaways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdNum = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Creator = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Stack = table.Column<bool>(type: "boolean", nullable: false),
                    WinnerAmount = table.Column<int>(type: "integer", nullable: false),
                    ReactionMessage = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LevelRequirement = table.Column<int>(type: "integer", nullable: true),
                    AccountAgeRequirement = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ServerAgeRequirement = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CloseAtOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Giveaways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: true),
                    Premium = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EmbedColor = table.Column<int>(type: "integer", nullable: false),
                    MvpChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameCustomChars",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Winner = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    SignUpChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    EventChannel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    EmoteMessageFormat = table.Column<string>(type: "text", nullable: true),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    SignUpStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SignUpMessage = table.Column<string>(type: "text", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false),
                    RoleReward = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameStatus", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "IgnoreChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoreChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "LevelConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    TextExpMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    VoiceExpMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    BoostExpMultiplier = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    Decay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExpDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    VoiceExpEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TextExpEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StackLvlRoles = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LevelExpEvents",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Multiplier = table.Column<double>(type: "double precision", nullable: false),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpEvents", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LevelExpReductions",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpReductions", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "LevelRewards",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Stackable = table.Column<bool>(type: "boolean", nullable: false),
                    NoDecay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelRewards", x => new { x.GuildId, x.Level });
                });

            migrationBuilder.CreateTable(
                name: "LoggingConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    LogJoin = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookJoin = table.Column<string>(type: "text", nullable: true),
                    WebhookJoinId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogMsg = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookMessage = table.Column<string>(type: "text", nullable: true),
                    WebhookMessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogBan = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookBan = table.Column<string>(type: "text", nullable: true),
                    WebhookBanId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogAvi = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookAvi = table.Column<string>(type: "text", nullable: true),
                    WebhookAviId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogWarn = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookWarn = table.Column<string>(type: "text", nullable: true),
                    WebhookWarnId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogAutoMod = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookAutoMod = table.Column<string>(type: "text", nullable: true),
                    WebhookAutoModId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    LogVoice = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookVoice = table.Column<string>(type: "text", nullable: true),
                    WebhookVoiceId = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggingConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStamp = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Logger = table.Column<string>(type: "text", nullable: true),
                    CallSite = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: true),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ModId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModLogs", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "MuteTimers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuteTimers", x => new { x.UserId, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "MvpConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Day = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    CreditReward = table.Column<int>(type: "integer", nullable: false),
                    SpecialCreditReward = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MvpConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Added = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Triggers = table.Column<List<string>>(type: "text[]", nullable: true),
                    LevelCap = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => new { x.GuildId, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Attachment = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignAbleRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    RoleId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    EmoteId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Exclusive = table.Column<bool>(type: "boolean", nullable: false),
                    EmoteReactFormat = table.Column<string>(type: "text", nullable: true),
                    EmoteMessageFormat = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignAbleRoles", x => new { x.GuildId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerStores",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    RoleId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    SpecialCredit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStores", x => new { x.GuildId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "SuggestionConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Channel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    EmoteYes = table.Column<string>(type: "text", nullable: true),
                    EmoteNo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    ResponseUser = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "VoiceRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    VoiceId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    RoleId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceRoles", x => new { x.GuildId, x.VoiceId });
                });

            migrationBuilder.CreateTable(
                name: "VoteLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Moderator = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Valid = table.Column<bool>(type: "boolean", nullable: false),
                    MuteTimer = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warns", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "WelcomeBanners",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Uploader = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    IsNsfw = table.Column<bool>(type: "boolean", nullable: false),
                    AvatarSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    AviPlaceX = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    AviPlaceY = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    TextSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 33),
                    TextPlaceX = table.Column<int>(type: "integer", nullable: false, defaultValue: 245),
                    TextPlaceY = table.Column<int>(type: "integer", nullable: false, defaultValue: 40),
                    UploadTimeOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeBanners", x => new { x.GuildId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "WelcomeConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Channel = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    WebhookId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Webhook = table.Column<string>(type: "text", nullable: true),
                    Limit = table.Column<int>(type: "integer", nullable: false),
                    Banner = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Reward = table.Column<int>(type: "integer", nullable: true),
                    TimeToDelete = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AutoDelOnLeave = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreNew = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementUnlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    AchieveId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountGlobalUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementUnlocks_AccountGlobals_AccountGlobalUserId",
                        column: x => x.AccountGlobalUserId,
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
                name: "SelfAssignReactionRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Reactions = table.Column<List<string>>(type: "text[]", nullable: true),
                    Exclusive = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ClubBlacklists",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    User = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Issuer = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "GiveawayParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Entry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GiveawayId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiveawayParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiveawayParticipants_Giveaways_GiveawayId",
                        column: x => x.GiveawayId,
                        principalTable: "Giveaways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignItems",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Exclusive = table.Column<bool>(type: "boolean", nullable: false),
                    EmoteId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Emote = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignItems", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_SelfAssignItems_SelfAssignGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SelfAssignGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "AchievementId", "Category", "Description", "Difficulty", "Hidden", "ImageUrl", "Name", "Points", "Requirement", "Reward" },
                values: new object[,]
                {
                    { new Guid("624da012-2194-428a-b693-b9b08ad0a9af"), 0, "Reach Server Level 5", 0, false, "", "Level 5", 10, 5, null },
                    { new Guid("aea7144b-e287-40ff-9a00-3a5a6b815170"), 0, "Reach Server Level 10", 0, false, "", "Level 10", 10, 10, null },
                    { new Guid("7c315a8d-35cc-48de-8dd4-f90d1d36132e"), 0, "Reach Server Level 20", 0, false, "", "Level 20", 10, 20, null },
                    { new Guid("915e030a-c71e-42fa-930a-ea250afb30aa"), 0, "Reach Server Level 30", 0, false, "", "Level 30", 10, 30, null },
                    { new Guid("2790c6d3-d8bf-47a8-a89e-bb018982deaf"), 0, "Reach Server Level 40", 0, false, "", "Level 40", 10, 40, null },
                    { new Guid("02d386b7-3ce5-442c-bd58-77a20dd4c5d3"), 0, "Reach Server Level 50", 1, false, "", "Level 50", 10, 50, null },
                    { new Guid("c9a5ef51-2bd8-430f-bd82-c725d6220ae7"), 0, "Reach Server Level 60", 1, false, "", "Level 60", 10, 60, null },
                    { new Guid("96785bc2-4de5-4d8d-9924-6c902729caf3"), 0, "Reach Server Level 70", 2, false, "", "Reach Server Level 70", 10, 70, null },
                    { new Guid("110319eb-d9d9-4fe0-a695-d8a1f839f5b0"), 0, "Reach Server Level 80", 2, false, "", "Level 80", 10, 80, null },
                    { new Guid("d33c6057-4128-4014-be25-ff9c08c0d9cc"), 0, "Reach Server Level 90", 3, false, "", "Level 90", 10, 90, null },
                    { new Guid("fe43f8c8-77eb-40ea-9b67-7ca8117160ba"), 0, "Reach Server Level 100", 3, false, "", "Level 100", 10, 100, null }
                });

            migrationBuilder.InsertData(
                table: "Backgrounds",
                columns: new[] { "Id", "BackgroundUrl" },
                values: new object[,]
                {
                    { new Guid("fa056244-ae59-4c3a-8cb3-ed79a7d782a4"), "https://i.imgur.com/OAMpNDh.png" },
                    { new Guid("fc53eac7-1af1-48b0-ba40-b16f5339ebe3"), "https://i.imgur.com/5ojmh76.png" },
                    { new Guid("2ae7e0a7-d01c-4b8a-8382-6c55fd867d77"), "https://i.imgur.com/04PbzvT.png" },
                    { new Guid("27b87558-d918-42b3-80a2-e0e9a8798f4a"), "https://i.imgur.com/epIb29P.png" },
                    { new Guid("6d1b1dad-6f2c-44b2-9d76-9eeebd1893d1"), "https://i.imgur.com/KXO5bx5.png" },
                    { new Guid("9a34da71-2f36-4570-9f3e-4bc6306c28ec"), "https://i.imgur.com/5h5zZ7C.png" }
                });

            migrationBuilder.InsertData(
                table: "HungerGameDefaults",
                columns: new[] { "Id", "Avatar", "Name" },
                values: new object[,]
                {
                    { 7ul, "https://i.imgur.com/VLsezdF.png", "Akagi" },
                    { 1ul, "https://i.imgur.com/XMjW8Qn.png", "Dia" },
                    { 2ul, "https://i.imgur.com/7URjbvT.png", "Kanan" },
                    { 3ul, "https://i.imgur.com/tPDON9P.png", "Yoshiko" },
                    { 4ul, "https://i.imgur.com/dcB1loo.png", "Kongou" },
                    { 25ul, "https://i.imgur.com/Wxhd5WY.png", "Shiro" },
                    { 24ul, "https://i.imgur.com/aijxHla.png", "Vanilla" },
                    { 23ul, "https://i.imgur.com/HoNwKi9.png", "Chocola" },
                    { 22ul, "https://i.imgur.com/bv5ao8Z.png", "Enterprise" },
                    { 21ul, "https://i.imgur.com/VyJf95i.png", "Bocchi" },
                    { 20ul, "https://i.imgur.com/GhSG97V.png", "Shiina" },
                    { 6ul, "https://i.imgur.com/8748bUL.png", "Yamato" },
                    { 19ul, "https://i.imgur.com/CI9Osi5.png", "Akame" },
                    { 17ul, "https://i.imgur.com/5xR0ImK.png", "Sora" },
                    { 16ul, "https://i.imgur.com/PT8SsVB.png", "Chika" },
                    { 15ul, "https://i.imgur.com/rYa5iYc.png", "Shiki" },
                    { 14ul, "https://i.imgur.com/0VYBYEg.png", "Gura" },
                    { 13ul, "https://i.imgur.com/5CcdVBE.png", "Ram" },
                    { 12ul, "https://i.imgur.com/y3bb8Sk.png", "Rem" },
                    { 11ul, "https://i.imgur.com/kF9b4SJ.png", "Emilia" },
                    { 5ul, "https://i.imgur.com/7GC7FvJ.png", "Haruna" },
                    { 9ul, "https://i.imgur.com/4XYg6ch.png", "Zero Two" },
                    { 8ul, "https://i.imgur.com/eyt9k8E.png", "Kaga" },
                    { 18ul, "https://i.imgur.com/U0NlfJd.png", "Nobuna" },
                    { 10ul, "https://i.imgur.com/Nl6WsbP.png", "Echidna" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementUnlocks_AccountGlobalUserId",
                table: "AchievementUnlocks",
                column: "AccountGlobalUserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_GiveawayParticipants_GiveawayId",
                table: "GiveawayParticipants",
                column: "GiveawayId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignItems_GroupId",
                table: "SelfAssignItems",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfAssignReactionRoles_ConfigId",
                table: "SelfAssignReactionRoles",
                column: "ConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "AdminConfigs");

            migrationBuilder.DropTable(
                name: "ApprovalQueues");

            migrationBuilder.DropTable(
                name: "AutoMessages");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Blacklists");

            migrationBuilder.DropTable(
                name: "BoardConfigs");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "BoostConfigs");

            migrationBuilder.DropTable(
                name: "ClubBlacklists");

            migrationBuilder.DropTable(
                name: "ClubConfigs");

            migrationBuilder.DropTable(
                name: "ClubPlayers");

            migrationBuilder.DropTable(
                name: "CurrencyConfigs");

            migrationBuilder.DropTable(
                name: "DblAuths");

            migrationBuilder.DropTable(
                name: "DropChannels");

            migrationBuilder.DropTable(
                name: "DropConfigs");

            migrationBuilder.DropTable(
                name: "GameClasses");

            migrationBuilder.DropTable(
                name: "GameConfigs");

            migrationBuilder.DropTable(
                name: "GameEnemies");

            migrationBuilder.DropTable(
                name: "GiveawayHistories");

            migrationBuilder.DropTable(
                name: "GiveawayParticipants");

            migrationBuilder.DropTable(
                name: "GuildConfigs");

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
                name: "IgnoreChannels");

            migrationBuilder.DropTable(
                name: "LevelConfigs");

            migrationBuilder.DropTable(
                name: "LevelExpEvents");

            migrationBuilder.DropTable(
                name: "LevelExpReductions");

            migrationBuilder.DropTable(
                name: "LevelRewards");

            migrationBuilder.DropTable(
                name: "LoggingConfigs");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "ModLogs");

            migrationBuilder.DropTable(
                name: "MuteTimers");

            migrationBuilder.DropTable(
                name: "MvpConfigs");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SelfAssignAbleRoles");

            migrationBuilder.DropTable(
                name: "SelfAssignItems");

            migrationBuilder.DropTable(
                name: "SelfAssignReactionRoles");

            migrationBuilder.DropTable(
                name: "ServerStores");

            migrationBuilder.DropTable(
                name: "SuggestionConfigs");

            migrationBuilder.DropTable(
                name: "Suggestions");

            migrationBuilder.DropTable(
                name: "VoiceRoles");

            migrationBuilder.DropTable(
                name: "VoteLogs");

            migrationBuilder.DropTable(
                name: "Warns");

            migrationBuilder.DropTable(
                name: "WelcomeBanners");

            migrationBuilder.DropTable(
                name: "WelcomeConfigs");

            migrationBuilder.DropTable(
                name: "AccountGlobals");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "ClubInfos");

            migrationBuilder.DropTable(
                name: "Giveaways");

            migrationBuilder.DropTable(
                name: "SelfAssignGroups");

            migrationBuilder.DropTable(
                name: "ChannelConfigs");
        }
    }
}
