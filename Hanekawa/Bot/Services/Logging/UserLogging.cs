using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUpdated(SocketUser before, SocketUser after)
        {
            _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }
    }
}
