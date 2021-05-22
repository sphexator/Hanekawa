using System;
using Hanekawa.Database.Entities.Items;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Database.Migrations
{
    public partial class MigrationAddStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("35c0c55e-1624-496e-80e3-6b749bce7ec9"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("44f7cfc4-4d23-44f8-8d21-09b5daad169d"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("4bd811cc-f752-4c3c-85a3-ed49e410f88e"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("52394c8d-f480-415e-94b8-705cd5785c41"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("9a8ab7a7-d522-402f-af50-92fa9abad84f"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("bb923ced-e792-4c72-aff0-667beb7f0b3d"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("c8612766-f919-4d35-abaa-10078baa7da3"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("c909b440-8777-4dc7-82e8-ea0f0255b3a3"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("cf99b550-9f40-4fae-af4f-d752c623764a"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("f67f05d9-6424-4eeb-b5e8-c16811cbea64"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("fd0261d9-5590-4dfd-ac44-d5b4ecd7f249"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("16924135-5103-42ed-8ce3-fca4a61bb179"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("26ca7657-c60f-4fe0-9f15-807fcd506e43"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("2787c44e-acfb-444a-ac38-beb72ad44919"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("39d2e876-de8d-4e76-a004-c373a0ffbb3e"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("95d9c246-6def-46f0-a8f7-f749a92b4c33"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("be1dc76c-23a9-4284-be37-dee68f957129"));

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DropColumn(
                name: "LogReaction",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "CriticalIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DamageIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "HealthIncrease",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Sell",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DesignChannel",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "EventChannel",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "EventSchedulerChannel",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "ModChannel",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "QuestionAndAnswerChannel",
                table: "ChannelConfigs");

            migrationBuilder.RenameColumn(
                name: "ReactionWebhook",
                table: "LoggingConfigs",
                newName: "WebhookWarn");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "IgnoreNew",
                table: "WelcomeConfigs",
                type: "interval",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Channel",
                table: "WelcomeConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "WelcomeConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Webhook",
                table: "WelcomeConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Uploader",
                table: "WelcomeBanners",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "WelcomeBanners",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Warns",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Warns",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "VoteLogs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "VoteLogs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "VoiceRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "VoiceId",
                table: "VoiceRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "VoiceRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Suggestions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ResponseUser",
                table: "Suggestions",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Suggestions",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Suggestions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Channel",
                table: "SuggestionConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "SuggestionConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "ServerStores",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ServerStores",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ConfigId",
                table: "SelfAssignReactionRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "SelfAssignReactionRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "SelfAssignReactionRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "SelfAssignReactionRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "EmoteId",
                table: "SelfAssignAbleRoles",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "SelfAssignAbleRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "SelfAssignAbleRoles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Reports",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Reports",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Reports",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Creator",
                table: "Quotes",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Quotes",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "MvpConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "MvpConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "CreditReward",
                table: "MvpConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExpReward",
                table: "MvpConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpecialCreditReward",
                table: "MvpConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "MuteTimers",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "MuteTimers",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "ModLogs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ModId",
                table: "ModLogs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "ModLogs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ModLogs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "LootChannels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LootChannels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "LogWarn",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogVoice",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogMsg",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogJoin",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogBan",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogAvi",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "LogAutoMod",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "WebhookAutoMod",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookAutoModId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookAvi",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookAviId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookBan",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookBanId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookJoin",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookJoinId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookMessage",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookMessageId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookVoice",
                table: "LoggingConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookVoiceId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WebhookWarnId",
                table: "LoggingConfigs",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Role",
                table: "LevelRewards",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LevelRewards",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "LevelExpReductions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LevelExpReductions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "LevelExpEvents",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "LevelExpEvents",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LevelExpEvents",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "LevelConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<IItem>(
                name: "ItemJson",
                table: "Items",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Inventories",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "IgnoreChannels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "IgnoreChannels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "SignUpChannel",
                table: "HungerGameStatus",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleReward",
                table: "HungerGameStatus",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "EventChannel",
                table: "HungerGameStatus",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "HungerGameStatus",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "HungerGames",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "HungerGameProfiles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "HungerGameProfiles",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Winner",
                table: "HungerGameHistories",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "HungerGameHistories",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "HungerGameDefaults",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "HungerGameCustomChars",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "HungerGameCustomChars",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Highlights",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Highlights",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MvpChannel",
                table: "GuildConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GuildConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Giveaways",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Creator",
                table: "Giveaways",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<ulong>(
                name: "ReactionMessage",
                table: "Giveaways",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "GiveawayParticipants",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GiveawayParticipants",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Creator",
                table: "GiveawayHistories",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GiveawayHistories",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "GameEnemies",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "EventPayouts",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "EventPayouts",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "DropConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleIdReward",
                table: "DblAuths",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "DblAuths",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "SpecialCurrencySignId",
                table: "CurrencyConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "CurrencySignId",
                table: "CurrencyConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "CurrencyConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "ClubPlayers",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ClubPlayers",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Role",
                table: "ClubInfos",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Leader",
                table: "ClubInfos",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ClubInfos",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Channel",
                table: "ClubInfos",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "AdMessage",
                table: "ClubInfos",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelCategory",
                table: "ClubConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "AdvertisementChannel",
                table: "ClubConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ClubConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Issuer",
                table: "ClubBlacklists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "User",
                table: "ClubBlacklists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ClubBlacklists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "SelfAssignableChannel",
                table: "ChannelConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ReportChannel",
                table: "ChannelConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ChannelConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "WebhookReport",
                table: "ChannelConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "BoostConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "BoostConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Webhook",
                table: "BoostConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Boards",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MessageId",
                table: "Boards",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Boards",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Channel",
                table: "BoardConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "BoardConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Webhook",
                table: "BoardConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "ResponsibleUser",
                table: "Blacklists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Blacklists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Creator",
                table: "AutoMessages",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "ChannelId",
                table: "AutoMessages",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "AutoMessages",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Webhook",
                table: "AutoMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Uploader",
                table: "ApprovalQueues",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "ApprovalQueues",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MuteRole",
                table: "AdminConfigs",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "AdminConfigs",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "AchievementUnlocks",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "AccountUserId",
                table: "AchievementUnlocks",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Accounts",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "Accounts",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "AccountGlobals",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "InventoryItem",
                columns: table => new
                {
                    ItemsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItem", x => new { x.ItemsId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_InventoryItem_Inventories_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Inventories",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "AchievementId", "Category", "Description", "Difficulty", "Hidden", "ImageUrl", "Name", "Points", "Requirement", "Reward" },
                values: new object[,]
                {
                    { new Guid("b783466d-41cd-466e-9493-e2c0ea9cb1df"), 0, "Reach Server Level 5", 0, false, "", "Level 5", 10, 5, null },
                    { new Guid("5a34b70b-738b-4bbe-9879-46cc94879ae0"), 0, "Reach Server Level 10", 0, false, "", "Level 10", 10, 10, null },
                    { new Guid("3c7a5b98-3e23-4926-be08-0ff60e15ce00"), 0, "Reach Server Level 20", 0, false, "", "Level 20", 10, 20, null },
                    { new Guid("756cae12-4fe9-43e7-ab08-b21eddea494a"), 0, "Reach Server Level 30", 0, false, "", "Level 30", 10, 30, null },
                    { new Guid("7a927920-6846-4255-8b85-442f17220482"), 0, "Reach Server Level 40", 0, false, "", "Level 40", 10, 40, null },
                    { new Guid("52e013bf-88f0-48cf-958a-e3fefb599146"), 0, "Reach Server Level 50", 1, false, "", "Level 50", 10, 50, null },
                    { new Guid("79c5a480-55de-4532-a8ed-ba8708385f3e"), 0, "Reach Server Level 60", 1, false, "", "Level 60", 10, 60, null },
                    { new Guid("266026f9-6b44-40f0-8fcd-896a129f461d"), 0, "Reach Server Level 70", 2, false, "", "Reach Server Level 70", 10, 70, null },
                    { new Guid("967d82f6-23ce-4511-93e7-314f8176a2cc"), 0, "Reach Server Level 80", 2, false, "", "Level 80", 10, 80, null },
                    { new Guid("e90cd59c-6d89-41ff-9b49-0b5df3295a61"), 0, "Reach Server Level 90", 3, false, "", "Level 90", 10, 90, null },
                    { new Guid("a6c4d454-8f52-4857-bd43-06117c52bfa3"), 0, "Reach Server Level 100", 3, false, "", "Level 100", 10, 100, null }
                });

            migrationBuilder.InsertData(
                table: "Backgrounds",
                columns: new[] { "Id", "BackgroundUrl" },
                values: new object[,]
                {
                    { new Guid("54bc322b-00e6-4649-a313-eea32eccf960"), "https://i.imgur.com/OAMpNDh.png" },
                    { new Guid("90485743-8cb6-45e0-ad6f-f3dd6f4909aa"), "https://i.imgur.com/5ojmh76.png" },
                    { new Guid("9e3e8fc6-fe28-4d58-a5b9-89f4c3b43db4"), "https://i.imgur.com/04PbzvT.png" },
                    { new Guid("33c231f4-f973-42c7-8e2c-59cb2454226a"), "https://i.imgur.com/epIb29P.png" },
                    { new Guid("fac4b8a9-aeaa-456a-b844-ee42d9900842"), "https://i.imgur.com/KXO5bx5.png" },
                    { new Guid("10175ccf-dd52-4a49-85ab-88a8eceb1b66"), "https://i.imgur.com/5h5zZ7C.png" }
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
                name: "IX_InventoryItem_UsersUserId",
                table: "InventoryItem",
                column: "UsersUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories");

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("266026f9-6b44-40f0-8fcd-896a129f461d"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("3c7a5b98-3e23-4926-be08-0ff60e15ce00"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("52e013bf-88f0-48cf-958a-e3fefb599146"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("5a34b70b-738b-4bbe-9879-46cc94879ae0"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("756cae12-4fe9-43e7-ab08-b21eddea494a"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("79c5a480-55de-4532-a8ed-ba8708385f3e"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("7a927920-6846-4255-8b85-442f17220482"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("967d82f6-23ce-4511-93e7-314f8176a2cc"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a6c4d454-8f52-4857-bd43-06117c52bfa3"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("b783466d-41cd-466e-9493-e2c0ea9cb1df"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("e90cd59c-6d89-41ff-9b49-0b5df3295a61"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("10175ccf-dd52-4a49-85ab-88a8eceb1b66"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("33c231f4-f973-42c7-8e2c-59cb2454226a"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("54bc322b-00e6-4649-a313-eea32eccf960"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("90485743-8cb6-45e0-ad6f-f3dd6f4909aa"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("9e3e8fc6-fe28-4d58-a5b9-89f4c3b43db4"));

            migrationBuilder.DeleteData(
                table: "Backgrounds",
                keyColumn: "Id",
                keyValue: new Guid("fac4b8a9-aeaa-456a-b844-ee42d9900842"));

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 1ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 2ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 3ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 4ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 5ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 6ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 7ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 8ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 9ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 10ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 11ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 12ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 13ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 14ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 15ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 16ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 17ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 18ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 19ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 20ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 21ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 22ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 23ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 24ul);

            migrationBuilder.DeleteData(
                table: "HungerGameDefaults",
                keyColumn: "Id",
                keyValue: 25ul);

            migrationBuilder.DropColumn(
                name: "Webhook",
                table: "WelcomeConfigs");

            migrationBuilder.DropColumn(
                name: "CreditReward",
                table: "MvpConfigs");

            migrationBuilder.DropColumn(
                name: "ExpReward",
                table: "MvpConfigs");

            migrationBuilder.DropColumn(
                name: "SpecialCreditReward",
                table: "MvpConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookAutoMod",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookAutoModId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookAvi",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookAviId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookBan",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookBanId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookJoin",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookJoinId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookMessage",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookMessageId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookVoice",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookVoiceId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "WebhookWarnId",
                table: "LoggingConfigs");

            migrationBuilder.DropColumn(
                name: "ItemJson",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ReactionMessage",
                table: "Giveaways");

            migrationBuilder.DropColumn(
                name: "WebhookReport",
                table: "ChannelConfigs");

            migrationBuilder.DropColumn(
                name: "Webhook",
                table: "BoostConfigs");

            migrationBuilder.DropColumn(
                name: "Webhook",
                table: "BoardConfigs");

            migrationBuilder.DropColumn(
                name: "Webhook",
                table: "AutoMessages");

            migrationBuilder.RenameColumn(
                name: "WebhookWarn",
                table: "LoggingConfigs",
                newName: "ReactionWebhook");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "IgnoreNew",
                table: "WelcomeConfigs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "interval",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Channel",
                table: "WelcomeConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "WelcomeConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Uploader",
                table: "WelcomeBanners",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "WelcomeBanners",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Warns",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Warns",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "VoteLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "VoteLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "VoiceRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "VoiceId",
                table: "VoiceRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "VoiceRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Suggestions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ResponseUser",
                table: "Suggestions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Suggestions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Suggestions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Channel",
                table: "SuggestionConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "SuggestionConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "ServerStores",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ServerStores",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ConfigId",
                table: "SelfAssignReactionRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "SelfAssignReactionRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "SelfAssignReactionRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "SelfAssignReactionRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "EmoteId",
                table: "SelfAssignAbleRoles",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "SelfAssignAbleRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "SelfAssignAbleRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Reports",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Reports",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "Quotes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Quotes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "MvpConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "MvpConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "MuteTimers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "MuteTimers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ModLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ModId",
                table: "ModLogs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "ModLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ModLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "LootChannels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LootChannels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "LogWarn",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogVoice",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogMsg",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogJoin",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogBan",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogAvi",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogAutoMod",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<long>(
                name: "LogReaction",
                table: "LoggingConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Role",
                table: "LevelRewards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelRewards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "LevelExpReductions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelExpReductions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "LevelExpEvents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "LevelExpEvents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelExpEvents",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "LevelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<int>(
                name: "CriticalIncrease",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DamageIncrease",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "Items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthIncrease",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Role",
                table: "Items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sell",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Inventories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "Inventories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "Inventories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "IgnoreChannels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "IgnoreChannels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "SignUpChannel",
                table: "HungerGameStatus",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RoleReward",
                table: "HungerGameStatus",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "EventChannel",
                table: "HungerGameStatus",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "HungerGameStatus",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "HungerGames",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "HungerGameProfiles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "HungerGameProfiles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Winner",
                table: "HungerGameHistories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "HungerGameHistories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HungerGameDefaults",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "HungerGameCustomChars",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HungerGameCustomChars",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Highlights",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Highlights",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MvpChannel",
                table: "GuildConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Giveaways",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "Giveaways",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "GiveawayParticipants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GiveawayParticipants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "GiveawayHistories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GiveawayHistories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GameEnemies",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "EventPayouts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "EventPayouts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "DropConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "RoleIdReward",
                table: "DblAuths",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "DblAuths",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "SpecialCurrencySignId",
                table: "CurrencyConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CurrencySignId",
                table: "CurrencyConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "CurrencyConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ClubPlayers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ClubPlayers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Role",
                table: "ClubInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Leader",
                table: "ClubInfos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ClubInfos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Channel",
                table: "ClubInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AdMessage",
                table: "ClubInfos",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ChannelCategory",
                table: "ClubConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AdvertisementChannel",
                table: "ClubConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ClubConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Issuer",
                table: "ClubBlacklists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "User",
                table: "ClubBlacklists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ClubBlacklists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "SelfAssignableChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ReportChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<long>(
                name: "DesignChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EventChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EventSchedulerChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ModChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QuestionAndAnswerChannel",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "BoostConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "BoostConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Boards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Boards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Boards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Channel",
                table: "BoardConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "BoardConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ResponsibleUser",
                table: "Blacklists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Blacklists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Creator",
                table: "AutoMessages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "AutoMessages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "AutoMessages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "Uploader",
                table: "ApprovalQueues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ApprovalQueues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "MuteRole",
                table: "AdminConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "AdminConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AchievementUnlocks",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "AccountUserId",
                table: "AchievementUnlocks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AccountGlobals",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "numeric(20,0)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                table: "Inventories",
                columns: new[] { "GuildId", "UserId", "ItemId" });

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
        }
    }
}
