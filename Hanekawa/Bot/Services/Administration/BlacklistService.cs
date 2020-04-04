using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Administration
{
    public class BlacklistService : INService, IRequired
    {
        private readonly DiscordClient _client;
        private readonly InternalLogService _log;

        public BlacklistService(DiscordClient client, InternalLogService log)
        {
            _client = client;
            _log = log;

            _client.JoinedGuild += _client_JoinedGuild;
        }

        private Task _client_JoinedGuild(JoinedGuildEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guild = e.Guild;
                try
                {
                    using (var db = new DbService())
                    {
                        var check = await db.Blacklists.FindAsync(guild.Id);
                        if (check == null) return;
                        await guild.LeaveAsync();
                        _log.LogAction(LogLevel.Information, $"Left {guild.Id} as the server is blacklisted");
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