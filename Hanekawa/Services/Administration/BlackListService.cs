using Discord.WebSocket;
using Hanekawa.Services.Entities;
using System.Threading.Tasks;

namespace Hanekawa.Services.Administration
{
    public class BlackListService
    {
        private readonly DiscordSocketClient _client;
        public BlackListService(DiscordSocketClient client)
        {
            _client = client;

            _client.JoinedGuild += BlacklistCheck;
        }

        private Task BlacklistCheck(SocketGuild guild)
        {
            var _ = Task.Run(async () =>
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
