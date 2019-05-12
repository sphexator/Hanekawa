using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Settings")]
    [Description("Server settings")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public partial class Settings : InteractiveBase
    {
        [Name("Add prefix")]
        [Command("")]
        [Description("Adds a prefix to the bot, if it doesn't already exist")]
        public async Task AddPrefixAsync([Remainder]string prefix)
        {

        }

        [Name("Remove prefix")]
        [Command("")]
        [Description("Removes a prefix from the bot, if it exists")]
        public async Task RemovePrefixAsync([Remainder]string prefix)
        {

        }

        [Name("Set embed color")]
        [Command("")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(uint color)
        {

        }
    }
}