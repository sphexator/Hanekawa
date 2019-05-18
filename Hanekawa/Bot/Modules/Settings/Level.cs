using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
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
    }
}