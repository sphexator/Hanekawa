using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;

namespace Hanekawa.Bot.Services.Administration
{
    public class BlacklistService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;

        public BlacklistService(DiscordSocketClient client)
        {
            _client = client;

            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var check = await db.Blacklists.FindAsync(guild.Id);
                    if (check == null) return;
                    await guild.LeaveAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}