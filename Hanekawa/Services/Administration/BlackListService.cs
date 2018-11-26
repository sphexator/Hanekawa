using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Administration
{
    public class BlackListService : IHanaService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public BlackListService(DiscordSocketClient client, DbService dbService)
        {
            _client = client;
            _db = dbService;

            _client.JoinedGuild += BlacklistCheck;
            Console.WriteLine("Blacklist service loaded");
        }

        private Task BlacklistCheck(SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                var check = await _db.Blacklists.FindAsync(guild.Id);
                if (check == null) return;
                await guild.LeaveAsync();
            });
            return Task.CompletedTask;
        }
    }
}