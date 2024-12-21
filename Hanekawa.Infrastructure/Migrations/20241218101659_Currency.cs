using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hanekawa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Currency",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "CurrencyConfig",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CurrencyName = table.Column<string>(type: "text", nullable: false),
                    CurrencySymbol = table.Column<string>(type: "text", nullable: false),
                    SymbolAffix = table.Column<int>(type: "integer", nullable: false),
                    IsEmote = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyConfig", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_CurrencyConfig_GuildConfigs_GuildId",
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
                name: "CurrencyConfig");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Users");
        }
    }
}
