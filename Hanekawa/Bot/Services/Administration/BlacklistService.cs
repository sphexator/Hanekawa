using System;
using System.Threading.Tasks;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Services.Administration
{
    public class BlacklistService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly NLog.Logger _log;
        private readonly IServiceProvider _provider;

        public BlacklistService(Hanekawa client, IServiceProvider provider)
        {
            _client = client;
            _log = LogManager.GetCurrentClassLogger();
            _provider = provider;

            _client.JoinedGuild += BlacklistCheck;
        }

        private Task BlacklistCheck(JoinedGuildEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guild = e.Guild;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var check = await db.Blacklists.FindAsync(guild.Id.RawValue);
                    if (check == null) return;
                    await guild.LeaveAsync();
                    _log.Log(LogLevel.Info, $"Left {guild.Id.RawValue} as the server is blacklisted");
                }
                catch (Exception e)
                {
                    _log.Log(NLog.LogLevel.Error, e, $"(Blacklist Service) Error for {guild.Id.RawValue} - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}