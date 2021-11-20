using System;
using System.Threading.Tasks;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.Administration
{
    public class BlacklistService : DiscordClientService
    {
        private readonly IServiceProvider _provider;

        public BlacklistService(IServiceProvider provider, Hanekawa bot, ILogger<BlacklistService> logger) : base(logger, bot) 
            => _provider = provider;

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var check = await db.Blacklists.FindAsync(e.GuildId);
                if (check == null) return;
                await e.Guild.LeaveAsync();
                Logger.LogInformation("Left {GuildId} as the server is blacklisted", e.GuildId);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "(Blacklist Service) Error for {GuildId} - {ExceptionMessage}", e.GuildId, exception.Message);
            }
        }
    }
}