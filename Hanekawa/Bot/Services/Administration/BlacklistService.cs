using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration
{
    public class BlacklistService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;

        public BlacklistService(DiscordSocketClient client, InternalLogService log)
        {
            _client = client;
            _log = log;

            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var check = await db.Blacklists.FindAsync(guild.Id);
                        if (check == null) return;
                        await guild.LeaveAsync();
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Blacklist Service) Error for {guild.Id} - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}