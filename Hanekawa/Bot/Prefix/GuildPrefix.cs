using System;
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
        private readonly IServiceProvider _provider;

        public GuildPrefix(IServiceProvider provider) => _provider = provider;

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            if (message.Channel is IPrivateChannel) return null;
            var prefixService = _provider.GetRequiredService<Services.Caching.CacheService>();
            var prefixCollection = prefixService.GetCollection(message.Guild.Id);
            if (prefixCollection != null)
            {
                prefixService.IsMentionPrefix(message, out _);
                return prefixCollection;
            }

            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(message.Guild.Id);
            var prefix = new StringPrefix(cfg.Prefix);
            prefixService.AddorUpdatePrefix(message.Guild, cfg.Prefix);
            prefixService.IsMentionPrefix(message, out _);
            var collection = prefixService.GetCollection(message.Guild.Id);

            return collection.Contains(prefix) 
                ? collection 
                : null;
        }
    }
}