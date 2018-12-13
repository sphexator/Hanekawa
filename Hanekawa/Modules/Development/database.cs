using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Modules.Development
{
    public class Database : InteractiveBase
    {
        [Command("srvcfg")]
        [RequireOwner]
        public async Task SetupServerConfig()
        {
            using (var db = new DbService())
            {
                await db.GetOrCreateGuildConfigAsync(Context.Guild);
                await ReplyAsync("Created or reseted guild config");
            }
        }
    }
}