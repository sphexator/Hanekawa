using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Prefix
{
    public class GuildPrefix : IPrefixProvider, INService
    {
        private readonly ConcurrentDictionary<Snowflake, HashSet<IPrefix>> _prefixCollection =
            new ConcurrentDictionary<Snowflake, HashSet<IPrefix>>();

        private readonly IServiceProvider _provider;

        public GuildPrefix(IServiceProvider provider) => _provider = provider;

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            if (message.Channel is IPrivateChannel) return null;
            if (_prefixCollection.TryGetValue(message.Guild.Id, out var prefixes)) return prefixes;

            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(message.Guild.Id);
            var prefix = new StringPrefix(cfg.Prefix);
            var collection = _prefixCollection.GetOrAdd(message.Guild.Id, new HashSet<IPrefix>());

            if (collection.Contains(prefix)) return collection;
            collection.Add(prefix);
            _prefixCollection.AddOrUpdate(message.Guild.Id, collection, (snowflake, set) => collection);
            return collection;
        }
    }
}