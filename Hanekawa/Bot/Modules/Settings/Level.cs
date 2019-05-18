using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Level")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Level : InteractiveBase
    {
        //TODO: Level stetings, all of them
        [Name("Level Reset")]
        [Command("level reset", "lvl reset")]
        [Description("Reset the server level/exp back to 0")]
        [Remarks("level reset")]
        [RequireServerOwner]
        public async Task ResetAsync()
        {
            await Context.ReplyAsync("");
        }

        [Name("Set Level")]
        [Command("set level", "set lvl")]
        [Description("Sets a user to a desired level")]
        [Remarks("set level @bob#0000 40")]
        [RequireServerOwner]
        public async Task SetLevelAsync(SocketGuildUser user, int level)
        {
        }

        [Name("Level Role Stack")]
        [Command("level role stack", "lvl stack")]
        [Description("Toggles between level roles stacking or keep the highest earned one")]
        [Remarks("level role stack")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task StackToggleAsync()
        {
        }

        [Name("Level Role Add")]
        [Command("level add", "lvl add")]
        [Description("Adds a role reward")]
        [Remarks("level add role 40")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddAsync(SocketRole role, int level)
        {
        }

        [Name("Level Role Remove")]
        [Command("level remove", "lvl remove")]
        [Description("Adds a role reward")]
        [Remarks("level add role 40")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveAsync(int level)
        {
        }

        private async Task AddLevelRole(HanekawaContext context, int level, IRole role, bool stack)
        {
            if (level <= 0) return;
            using (var db = new DbService())
            {
                var check = await db.LevelRewards.FindAsync(context.Guild.Id, level);
                if (check != null)
                {
                    var gRole = context.Guild.GetRole(check.Role);
                    if (gRole != null)
                    {
                        await context.ReplyAsync($"Do you wish to replace {gRole.Name} for level {check.Level}? (y/n)");
                        var response = await NextMessageAsync();
                        if (response.Content.ToLower() != "y" || response.Content.ToLower() != "yes")
                        {
                            await context.ReplyAsync("Cancelling.");
                            return;
                        }
                    }
                }

                var data = new LevelReward
                {
                    GuildId = context.Guild.Id,
                    Level = level,
                    Role = role.Id,
                    Stackable = stack
                };
                await db.LevelRewards.AddAsync(data);
                await db.SaveChangesAsync();
                await context.ReplyAsync($"Added {role.Name} as a lvl{level} reward!", Color.Green.RawValue);
            }
        }
    }
}