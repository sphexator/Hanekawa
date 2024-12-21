using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hanekawa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLatestStuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelUpDmEnabled",
                table: "LevelConfig");

            migrationBuilder.DropColumn(
                name: "LevelUpDmMessage",
                table: "LevelConfig");

            migrationBuilder.DropColumn(
                name: "LevelUpMessage",
                table: "LevelConfig");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "GreetConfig");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LevelUpMessageEnabled",
                table: "LevelConfig",
                newName: "DecayEnabled");

            migrationBuilder.AddColumn<long>(
                name: "CurrentLevelExperience",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NextLevelExperience",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Multiplier",
                table: "LevelConfig",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MultiplierEnd",
                table: "LevelConfig",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "AvatarSize",
                table: "GreetImage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvatarX",
                table: "GreetImage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvatarY",
                table: "GreetImage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "UsernameSize",
                table: "GreetImage",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "UsernameX",
                table: "GreetImage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsernameY",
                table: "GreetImage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdminConfig",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MaxWarnings = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminConfig", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_AdminConfig_GuildConfigs_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildConfigs",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DropConfig",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Emote = table.Column<string>(type: "text", nullable: false),
                    ExpReward = table.Column<int>(type: "integer", nullable: false),
                    Blacklist = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropConfig", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_DropConfig_GuildConfigs_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildConfigs",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevelRequirements",
                columns: table => new
                {
                    Level = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Experience = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelRequirements", x => x.Level);
                });

            migrationBuilder.CreateTable(
                name: "LevelReward",
                columns: table => new
                {
                    Level = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Money = table.Column<int>(type: "integer", nullable: true),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelReward", x => x.Level);
                    table.ForeignKey(
                        name: "FK_LevelReward_LevelConfig_GuildId",
                        column: x => x.GuildId,
                        principalTable: "LevelConfig",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogConfig",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JoinLeaveLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MessageLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ModLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    VoiceLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogConfig", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_LogConfig_GuildConfigs_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildConfigs",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ModeratorId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationLogs", x => new { x.GuildId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ModeratorId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LevelReward_GuildId",
                table: "LevelReward",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminConfig");

            migrationBuilder.DropTable(
                name: "DropConfig");

            migrationBuilder.DropTable(
                name: "LevelRequirements");

            migrationBuilder.DropTable(
                name: "LevelReward");

            migrationBuilder.DropTable(
                name: "LogConfig");

            migrationBuilder.DropTable(
                name: "ModerationLogs");

            migrationBuilder.DropTable(
                name: "Warnings");

            migrationBuilder.DropColumn(
                name: "CurrentLevelExperience",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NextLevelExperience",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Multiplier",
                table: "LevelConfig");

            migrationBuilder.DropColumn(
                name: "MultiplierEnd",
                table: "LevelConfig");

            migrationBuilder.DropColumn(
                name: "AvatarSize",
                table: "GreetImage");

            migrationBuilder.DropColumn(
                name: "AvatarX",
                table: "GreetImage");

            migrationBuilder.DropColumn(
                name: "AvatarY",
                table: "GreetImage");

            migrationBuilder.DropColumn(
                name: "UsernameSize",
                table: "GreetImage");

            migrationBuilder.DropColumn(
                name: "UsernameX",
                table: "GreetImage");

            migrationBuilder.DropColumn(
                name: "UsernameY",
                table: "GreetImage");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "User",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "DecayEnabled",
                table: "LevelConfig",
                newName: "LevelUpMessageEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "LevelUpDmEnabled",
                table: "LevelConfig",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LevelUpDmMessage",
                table: "LevelConfig",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LevelUpMessage",
                table: "LevelConfig",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "GreetConfig",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
