using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Services.Entities;

namespace Hanekawa.Services.Administration
{
    public class BlackList
    {
        private readonly DiscordSocketClient _client;
        public BlackList(DiscordSocketClient client)
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
                    var check = db.Blacklists.FindAsync(guild.Id);
                    if (check == null) return;
                    await guild.LeaveAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}
