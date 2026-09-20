using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hanekawa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityConfig",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PreviousWeekRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    CurrentWeekRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    CurrentWeekTopAmount = table.Column<int>(type: "integer", nullable: false),
                    AnnouncementChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    AnnouncementMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentHolders = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: false),
                    PreviousWeekHolderId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    LastProcessedWeekStart = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityConfig", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_ActivityConfig_GuildConfigs_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildConfigs",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityConfig");
        }
    }
}
