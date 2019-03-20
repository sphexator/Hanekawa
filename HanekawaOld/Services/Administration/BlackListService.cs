using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Administration
{
    public class BlackListService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;

        public BlackListService(DiscordSocketClient client)
        {
            _client = client;

            _client.JoinedGuild += BlacklistCheck;
            Console.WriteLine("Blacklist service loaded");
        }

        private static Task BlacklistCheck(SocketGuild guild)
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