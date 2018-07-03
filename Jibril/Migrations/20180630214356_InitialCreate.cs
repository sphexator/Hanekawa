using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jibril.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Credit = table.Column<uint>(nullable: false),
                    CreditSpecial = table.Column<uint>(nullable: false),
                    Level = table.Column<uint>(nullable: false),
                    Exp = table.Column<uint>(nullable: false),
                    TotalExp = table.Column<uint>(nullable: false),
                    VoiceExpTime = table.Column<DateTime>(nullable: false),
                    DailyCredit = table.Column<DateTime>(nullable: false),
                    Class = table.Column<string>(nullable: true),
                    ProfilePic = table.Column<string>(nullable: true),
                    CustomRoleId = table.Column<ulong>(nullable: true),
                    MvpCounter = table.Column<uint>(nullable: false),
                    MvpIgnore = table.Column<bool>(nullable: false),
                    MvpImmunity = table.Column<bool>(nullable: false),
                    Rep = table.Column<uint>(nullable: false),
                    RepCooldown = table.Column<DateTime>(nullable: false),
                    LastMessage = table.Column<DateTime>(nullable: false),
                    FirstMessage = table.Column<DateTime>(nullable: true),
                    Sessions = table.Column<uint>(nullable: false),
                    TimeInVoice = table.Column<TimeSpan>(nullable: false),
                    VoiceTime = table.Column<DateTime>(nullable: false),
                    GameKillAmount = table.Column<uint>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ClubInfos",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Leader = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    Channel = table.Column<ulong>(nullable: true),
                    RoleId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubPlayers",
                columns: table => new
                {
                    ClubId = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Rank = table.Column<uint>(nullable: false),
                    JoinDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPlayers", x => x.ClubId);
                });

            migrationBuilder.CreateTable(
                name: "GameEnemies",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    Health = table.Column<uint>(nullable: false),
                    Damage = table.Column<uint>(nullable: false),
                    Class = table.Column<string>(nullable: true),
                    ExpGain = table.Column<uint>(nullable: false),
                    CreditGain = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEnemies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Welcome = table.Column<bool>(nullable: false),
                    WelcomeChannel = table.Column<ulong>(nullable: false),
                    WelcomeLimit = table.Column<uint>(nullable: false),
                    LogJoin = table.Column<ulong>(nullable: true),
                    LogMsg = table.Column<ulong>(nullable: true),
                    LogBan = table.Column<ulong>(nullable: true),
                    LogAvi = table.Column<ulong>(nullable: true),
                    AntiSpam = table.Column<uint>(nullable: true),
                    ExpMultiplier = table.Column<uint>(nullable: false),
                    StackLvlRoles = table.Column<bool>(nullable: false),
                    MuteRole = table.Column<ulong>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "GuildInfos",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RuleMessageId = table.Column<ulong>(nullable: false),
                    Rules = table.Column<string>(nullable: true),
                    FaqOneMessageId = table.Column<ulong>(nullable: false),
                    FaqOne = table.Column<string>(nullable: true),
                    FaqTwoMessageId = table.Column<ulong>(nullable: false),
                    FaqTwo = table.Column<string>(nullable: true),
                    StaffMessageId = table.Column<ulong>(nullable: false),
                    LevelMessageId = table.Column<ulong>(nullable: false),
                    InviteMessageId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInfos", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameConfigs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<ulong>(nullable: false),
                    SignupStage = table.Column<bool>(nullable: false),
                    Live = table.Column<bool>(nullable: false),
                    Round = table.Column<uint>(nullable: false),
                    SignupTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameDefaults",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameDefaults", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "HungerGameLives",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Health = table.Column<uint>(nullable: false),
                    Stamina = table.Column<uint>(nullable: false),
                    Fatigue = table.Column<uint>(nullable: false),
                    Hunger = table.Column<uint>(nullable: false),
                    Thirst = table.Column<uint>(nullable: false),
                    Sleep = table.Column<uint>(nullable: false),
                    Bleeding = table.Column<bool>(nullable: false),
                    Food = table.Column<uint>(nullable: false),
                    Water = table.Column<uint>(nullable: false),
                    TotalWeapons = table.Column<uint>(nullable: false),
                    Pistol = table.Column<uint>(nullable: false),
                    Axe = table.Column<uint>(nullable: false),
                    Bow = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HungerGameLives", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LevelRewards",
                columns: table => new
                {
                    Level = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Role = table.Column<ulong>(nullable: false),
                    Stackable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelRewards", x => x.Level);
                });

            migrationBuilder.CreateTable(
                name: "ModLogs",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(nullable: false),
                    Action = table.Column<string>(nullable: true),
                    MessageId = table.Column<ulong>(nullable: false),
                    ModId = table.Column<ulong>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuteTimers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuteTimers", x => x.UserId);
                    table.UniqueConstraint("AK_MuteTimers_GuildId", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "NudeServiceChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Tolerance = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NudeServiceChannels", x => x.ChannelId);
                    table.UniqueConstraint("AK_NudeServiceChannels_GuildId", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<ulong>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Attachment = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.UserId);
                    table.UniqueConstraint("AK_Reports_MessageId", x => x.MessageId);
                });

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

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Item = table.Column<string>(nullable: true),
                    Price = table.Column<uint>(nullable: false),
                    RoleId = table.Column<ulong>(nullable: true),
                    ROle = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                columns: table => new
                {
                    Id = table.Column<uint>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    MessageId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ResponseUser = table.Column<ulong>(nullable: false),
                    Response = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => x.MessageId);
                    table.UniqueConstraint("AK_Suggestions_UserId", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "WarnMsgLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WarnId = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    MsgId = table.Column<ulong>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarnMsgLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    Moderator = table.Column<ulong>(nullable: false),
                    Valid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RepairKit = table.Column<uint>(nullable: false),
                    DamageBoost = table.Column<uint>(nullable: false),
                    Shield = table.Column<uint>(nullable: false),
                    CustomRole = table.Column<uint>(nullable: false),
                    Gift = table.Column<uint>(nullable: false),
                    AccountUserId = table.Column<ulong>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Inventory_Accounts_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_AccountUserId",
                table: "Inventory",
                column: "AccountUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubInfos");

            migrationBuilder.DropTable(
                name: "ClubPlayers");

            migrationBuilder.DropTable(
                name: "GameEnemies");

            migrationBuilder.DropTable(
                name: "GuildConfigs");

            migrationBuilder.DropTable(
                name: "GuildInfos");

            migrationBuilder.DropTable(
                name: "HungerGameConfigs");

            migrationBuilder.DropTable(
                name: "HungerGameDefaults");

            migrationBuilder.DropTable(
                name: "HungerGameLives");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "LevelRewards");

            migrationBuilder.DropTable(
                name: "ModLogs");

            migrationBuilder.DropTable(
                name: "MuteTimers");

            migrationBuilder.DropTable(
                name: "NudeServiceChannels");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ShopEvents");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Suggestions");

            migrationBuilder.DropTable(
                name: "WarnMsgLogs");

            migrationBuilder.DropTable(
                name: "Warns");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
