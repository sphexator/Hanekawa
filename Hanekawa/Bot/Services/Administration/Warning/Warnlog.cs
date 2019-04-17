using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.Administration.Warning
{
    public partial class WarnService
    {
        public async Task<EmbedBuilder> GetSimpleWarnlogAsync(SocketGuildUser user)
        {
            return null;
        }

        public async Task<List<string>> GetFullWarnlogAsync(SocketGuildUser user)
        {
            return null;
        }
    }
}
