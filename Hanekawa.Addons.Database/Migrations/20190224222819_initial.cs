using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Hanekawa.Database.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGlobals",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Exp = table.Column<int>(nullable: false),
                    TotalExp = table.Column<int>(nullable: false),
                    Rep = table.Column<int>(nullable: false),
                    Credit = table.Column<int>(nullable: false),
                    StarReceive = table.Column<int>(nullable: false),
                    StarGive = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGlobals", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Credit = table.Column<int>(nullable: false),
                    CreditSpecial = table.Column<int>(nullable: false),
                    DailyCredit = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Exp = table.Column<int>(nullable: false),
                    TotalExp = table.Column<int>(nullable: false),
                    VoiceExpTime = table.Column<DateTime>(nullable: false),
                    Class = table.Column<int>(nullable: false),
                    ProfilePic = table.Column<string>(nullable: true),
                    Rep = table.Column<int>(nullable: false),
                    RepCooldown = table.Column<DateTime>(nullable: false),
                    GameKillAmount = table.Column<int>(nullable: false),
                    FirstMessage = table.Column<DateTime>(nullable: true),
                    LastMessage = table.Column<DateTime>(nullable: false),
                    StatVoiceTime = table.Column<TimeSpan>(nullable: false),
                    Sessions = table.Column<int>(nullable: false),
                    StatMessages = table.Column<long>(nullable: false),
                    StarGiven = table.Column<int>(nullable: false),
                    StarReceived = table.Column<int>(nullable: false),
                    ChannelVoiceTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "AchievementDifficulties",
                columns: table => new
                {
                    DifficultyId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                    UserId = table.Column<long>(nullable: false),
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTypes", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "AdminConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    MuteRole = table.Column<long>(nullable: true),
                    FilterInvites = table.Column<bool>(nullable: false),
                    IgnoreAllChannels = table.Column<bool>(nullable: false),
                    FilterMsgLength = table.Column<int>(nullable: true),
                    FilterUrls = table.Column<bool>(nullable: false),
                    FilterAllInv = table.Column<bool>(nullable: false),
                    EmoteCountFilter = table.Column<int>(nullable: true),
                    MentionCountFilter = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BackgroundUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blacklists",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    ResponsibleUser = table.Column<long>(nullable: false),
                    Unban = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "BoardConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Emote = table.Column<string>(nullable: true),
                    Channel = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    MessageId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    StarAmount = table.Column<int>(nullable: false),
                    Boarded = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => new { x.GuildId, x.MessageId });
                });

            migrationBuilder.CreateTable(
                name: "ChannelConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ReportChannel = table.Column<long>(nullable: true),
                    EventChannel = table.Column<long>(nullable: true),
                    EventSchedulerChannel = table.Column<long>(nullable: true),
                    ModChannel = table.Column<long>(nullable: true),
                    DesignChannel = table.Column<long>(nullable: true),
                    QuestionAndAnswerChannel = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ClubBlacklists",
                columns: table => new
                {
                    ClubId = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    BlackListUser = table.Column<long>(nullable: false),
                    IssuedUser = table.Column<long>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Time = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubBlacklists", x => new { x.ClubId, x.GuildId, x.BlackListUser });
                });

            migrationBuilder.CreateTable(
                name: "ClubConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelCategory = table.Column<long>(nullable: true),
                    AdvertisementChannel = table.Column<long>(nullable: true),
                    EnableVoiceChannel = table.Column<bool>(nullable: false),
                    ChannelRequiredAmount = table.Column<int>(nullable: false),
                    ChannelRequiredLevel = table.Column<int>(nullable: false),
                    AutoPrune = table.Column<bool>(nullable: false),
                    RoleEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "ClubInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    LeaderId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IconUrl = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    AutoAdd = table.Column<bool>(nullable: false),
                    AdMessage = table.Column<long>(nullable: true),
                    Channel = table.Column<long>(nullable: true),
                    Role = table.Column<long>(nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    ClubId = table.Column<int>(nullable: false),
                    Rank = table.Column<int>(nullable: false),
                    JoinDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    EmoteCurrency = table.Column<bool>(nullable: false, defaultValue: false),
                    CurrencyName = table.Column<string>(nullable: true),
                    CurrencySign = table.Column<string>(nullable: true),
                    SpecialEmoteCurrency = table.Column<bool>(nullable: false, defaultValue: false),
                    SpecialCurrencyName = table.Column<string>(nullable: true),
                    SpecialCurrencySign = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "EventPayouts",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPayouts", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "EventSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Host = table.Column<long>(nullable: false),
                    DesignerClaim = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    Posted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedules", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "GameClasses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    LevelRequirement = table.Column<long>(nullable: false),
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DefaultHealth = table.Column<int>(nullable: false),
                    DefaultDamage = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameEnemies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Elite = table.Column<bool>(nullable: false),
                    Rare = table.Column<bool>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    Health = table.Column<int>(nullable: false),
                    Damage = table.Column<int>(nullable: false),
                    ClassId = table.Column<int>(nullable: false),
                    ExpGain = table.Column<int>(nullable: false),
                    CreditGain = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEnemies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    Premium = table.Column<bool>(nullable: false, defaultValue: false),
                    EmbedColor = table.Column<int>(nullable: false),
                    AnimeAirChannel = table.Column<long>(nullable: true),
                    AutomaticEventSchedule = table.Column<bool>(nullable: false),
                    MusicChannel = table.Column<long>(nullable: true),
                    MusicVcChannel = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "IgnoreChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoreChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    ItemId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => new { x.GuildId, x.UserId, x.ItemId });
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Role = table.Column<long>(nullable: false),
                    DateAdded = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LevelConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ExpMultiplier = table.Column<int>(nullable: false),
                    StackLvlRoles = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LevelExpEvents",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: true),
                    MessageId = table.Column<long>(nullable: true),
                    Multiplier = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpEvents", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LevelExpReductions",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    Channel = table.Column<bool>(nullable: false),
                    Category = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelExpReductions", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "LevelRewards",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Role = table.Column<long>(nullable: false),
                    Stackable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelRewards", x => new { x.GuildId, x.Level });
                });

            migrationBuilder.CreateTable(
                name: "LoggingConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    LogJoin = table.Column<long>(nullable: true),
                    LogMsg = table.Column<long>(nullable: true),
                    LogBan = table.Column<long>(nullable: true),
                    LogAvi = table.Column<long>(nullable: true),
                    LogWarn = table.Column<long>(nullable: true),
                    LogAutoMod = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggingConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LootChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "ModLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Action = table.Column<string>(nullable: true),
                    MessageId = table.Column<long>(nullable: false),
                    ModId = table.Column<long>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModLogs", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "MuteTimers",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuteTimers", x => new { x.UserId, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "NudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    Tolerance = table.Column<int>(nullable: false),
                    InHouse = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NudeServiceChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "ProfileConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Height = table.Column<float>(nullable: false),
                    NameWidth = table.Column<float>(nullable: false),
                    ValueWidth = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAndAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    MessageId = table.Column<long>(nullable: true),
                    ResponseUser = table.Column<long>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAndAnswers", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    MessageId = table.Column<long>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Attachment = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignAbleRoles",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: false),
                    Exclusive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignAbleRoles", x => new { x.GuildId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "ServerStores",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: false),
                    Price = table.Column<int>(nullable: false),
                    SpecialCredit = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStores", x => new { x.GuildId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "SingleNudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    Level = table.Column<int>(nullable: true),
                    Tolerance = table.Column<int>(nullable: true),
                    InHouse = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleNudeServiceChannels", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "SpamIgnores",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpamIgnores", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "SuggestionConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Channel = table.Column<long>(nullable: true),
                    EmoteYes = table.Column<string>(nullable: true),
                    EmoteNo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    MessageId = table.Column<long>(nullable: true),
                    ResponseUser = table.Column<long>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "UrlFilters",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFilters", x => new { x.GuildId, x.ChannelId });
                });

            migrationBuilder.CreateTable(
                name: "Warns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    Moderator = table.Column<decimal>(nullable: false),
                    Valid = table.Column<bool>(nullable: false),
                    MuteTimer = table.Column<TimeSpan>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warns", x => new { x.Id, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "WelcomeBanners",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Url = table.Column<string>(nullable: true),
                    Uploader = table.Column<long>(nullable: false),
                    UploadTimeOffset = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeBanners", x => new { x.GuildId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "WelcomeConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    Channel = table.Column<long>(nullable: true),
                    Limit = table.Column<int>(nullable: false),
                    Banner = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    TimeToDelete = table.Column<TimeSpan>(nullable: true),
                    AutoDelOnLeave = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "WhitelistDesigns",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistDesigns", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "WhitelistEvents",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistEvents", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                    UserId = table.Column<long>(nullable: false),
                    TypeId = table.Column<int>(nullable: false)
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
                name: "AccountGlobals");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AchievementTrackers");

            migrationBuilder.DropTable(
                name: "AchievementUnlocks");

            migrationBuilder.DropTable(
                name: "AdminConfigs");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Blacklists");

            migrationBuilder.DropTable(
                name: "BoardConfigs");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "ChannelConfigs");

            migrationBuilder.DropTable(
                name: "ClubBlacklists");

            migrationBuilder.DropTable(
                name: "ClubConfigs");

            migrationBuilder.DropTable(
                name: "ClubInfos");

            migrationBuilder.DropTable(
                name: "ClubPlayers");

            migrationBuilder.DropTable(
                name: "CurrencyConfigs");

            migrationBuilder.DropTable(
                name: "EventPayouts");

            migrationBuilder.DropTable(
                name: "EventSchedules");

            migrationBuilder.DropTable(
                name: "GameClasses");

            migrationBuilder.DropTable(
                name: "GameConfigs");

            migrationBuilder.DropTable(
                name: "GameEnemies");

            migrationBuilder.DropTable(
                name: "GuildConfigs");

            migrationBuilder.DropTable(
                name: "IgnoreChannels");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Items");

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
                name: "LootChannels");

            migrationBuilder.DropTable(
                name: "ModLogs");

            migrationBuilder.DropTable(
                name: "MuteTimers");

            migrationBuilder.DropTable(
                name: "NudeServiceChannels");

            migrationBuilder.DropTable(
                name: "ProfileConfigs");

            migrationBuilder.DropTable(
                name: "QuestionAndAnswers");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "SelfAssignAbleRoles");

            migrationBuilder.DropTable(
                name: "ServerStores");

            migrationBuilder.DropTable(
                name: "SingleNudeServiceChannels");

            migrationBuilder.DropTable(
                name: "SpamIgnores");

            migrationBuilder.DropTable(
                name: "SuggestionConfigs");

            migrationBuilder.DropTable(
                name: "Suggestions");

            migrationBuilder.DropTable(
                name: "UrlFilters");

            migrationBuilder.DropTable(
                name: "Warns");

            migrationBuilder.DropTable(
                name: "WelcomeBanners");

            migrationBuilder.DropTable(
                name: "WelcomeConfigs");

            migrationBuilder.DropTable(
                name: "WhitelistDesigns");

            migrationBuilder.DropTable(
                name: "WhitelistEvents");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementNames");

            migrationBuilder.DropTable(
                name: "AchievementDifficulties");

            migrationBuilder.DropTable(
                name: "AchievementTypes");
        }
    }
}
