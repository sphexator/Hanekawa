using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;

namespace Hanekawa.Bot.Services.Administration
{
    public class BlacklistService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public BlacklistService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                var check = await _db.Blacklists.FindAsync(guild.Id);
                if (check == null) return;
                await guild.LeaveAsync();
            });
            return Task.CompletedTask;
        }
    }
}