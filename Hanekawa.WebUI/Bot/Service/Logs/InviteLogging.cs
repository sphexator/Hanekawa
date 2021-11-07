using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.WebUI.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnInviteCreated(InviteCreatedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(e.GuildId.Value);
            if (!cfg.LogJoin.HasValue) return;
            _cache.AddInvite(e.GuildId.Value, e.Inviter.Id, e.Code, e.Uses);
        }

        protected override async ValueTask OnInviteDeleted(InviteDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(e.GuildId.Value);
            if (!cfg.LogJoin.HasValue) return;
            _cache.RemoveInvite(e.GuildId.Value, e.Code);
        }
    }
}