using System;
using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.WebUI.Bot.Service.Administration
{
    public class BlacklistService : DiscordClientService
    {
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;

        public BlacklistService(IServiceProvider provider, Hanekawa bot, ILogger<BlacklistService> logger) : base(logger, bot)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _provider = provider;
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var check = await db.Blacklists.FindAsync(e.GuildId);
                if (check == null) return;
                await e.Guild.LeaveAsync();
                _logger.Log(LogLevel.Info, $"Left {e.GuildId} as the server is blacklisted");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"(Blacklist Service) Error for {e.GuildId} - {exception.Message}");
            }
        }
    }
}