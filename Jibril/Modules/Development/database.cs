using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Extensions;
using Jibril.Services.Entities;

namespace Jibril.Modules.Development
{
    public class Database : InteractiveBase
    {
        [Command("srvcfg")]
        [RequireOwner]
        public async Task SetupServerConfig()
        {
            using (var db = new DbService())
            {
                await db.GetOrCreateGuildConfig(Context.Guild);
                await ReplyAsync("Created or reseted guild config");
            }
        }
    }
}
