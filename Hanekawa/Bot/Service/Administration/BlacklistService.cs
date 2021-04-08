using System;
using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Administration
{
    public class BlacklistService : INService
    {
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;

        public BlacklistService(IServiceProvider provider)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _provider = provider;
        }

        public async Task BlackListAsync(JoinedGuildEventArgs e)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var check = await db.Blacklists.FindAsync(e.GuildId.RawValue);
                if (check == null) return;
                await e.Guild.LeaveAsync();
                _logger.Log(LogLevel.Info, $"Left {e.GuildId.RawValue} as the server is blacklisted");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"(Blacklist Service) Error for {e.GuildId.RawValue} - {exception.Message}");
            }
        }
    }
}
