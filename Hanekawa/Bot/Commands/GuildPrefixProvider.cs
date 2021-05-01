using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class GuildPrefixProvider : IPrefixProvider, INService
    {
        private readonly IServiceProvider _provider;

        public GuildPrefixProvider(IServiceProvider provider) => _provider = provider;

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
        {
            if (!message.GuildId.HasValue) throw new InvalidOperationException("Bot cannot be used in dms");
            var cache = _provider.GetRequiredService<CacheService>();
            if (cache.GuildPrefix.TryGetValue(message.GuildId.Value, out var collection)) return collection;

            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(message.GuildId.Value);
            collection = new HashSet<IPrefix> {new StringPrefix(cfg.Prefix)};
            cache.GuildPrefix.AddOrUpdate(message.GuildId.Value, collection, (_, _) => collection);
            return collection;
        }
    }
}