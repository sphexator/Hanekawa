using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        public async ValueTask InviteCreatedAsync(InviteCreatedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(e.GuildId.Value);
            if (!cfg.LogJoin.HasValue) return;
            var invites = _cache.GuildInvites.GetOrAdd(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            invites.TryAdd(e.Code, new Tuple<Snowflake?, int>(e.Invite.Inviter.Value.Id.RawValue, 0));
            _cache.GuildInvites.AddOrUpdate(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>(),
                (_, _) => invites);
        }

        public async ValueTask InviteDeletedAsync(InviteDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(e.GuildId.Value.RawValue);
            if (!cfg.LogJoin.HasValue) return;
            var invites = _cache.GuildInvites.GetOrAdd(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>());
            invites.TryRemove(e.Code, out _);
            _cache.GuildInvites.AddOrUpdate(e.GuildId.Value.RawValue, new ConcurrentDictionary<string, Tuple<Snowflake?, int>>(),
                (_, _) => invites);
        }
    }
}