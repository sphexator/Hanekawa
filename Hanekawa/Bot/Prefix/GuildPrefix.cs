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
using Qmmands;

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
            IPrefix mentionPrefix = null;
            if (message.Channel is IPrivateChannel) return null;
            if (_prefixCollection.TryGetValue(message.Guild.Id, out var prefixes))
            {
                if (!IsMentionPrefix(message, out mentionPrefix)) return prefixes;
                if (!prefixes.Contains(mentionPrefix)) prefixes.Add(mentionPrefix);
                return prefixes;
            }

            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(message.Guild.Id);
            var prefix = new StringPrefix(cfg.Prefix);
            var collection = _prefixCollection.GetOrAdd(message.Guild.Id, new HashSet<IPrefix>(new []{prefix}));
            if (IsMentionPrefix(message, out mentionPrefix))
            {
                if (!collection.Contains(mentionPrefix)) collection.Add(mentionPrefix);
                _prefixCollection.AddOrUpdate(message.Guild.Id, collection, (snowflake, set) => collection);
            }
            if (collection.Contains(prefix)) return collection;
            collection.Add(prefix);
            _prefixCollection.AddOrUpdate(message.Guild.Id, collection, (snowflake, set) => collection);
            return collection;
        }

        private static bool IsMentionPrefix(CachedUserMessage message, out IPrefix mentionPrefix)
        {
            var contentSpan = message.Content.AsSpan();
            if (contentSpan.Length > 17
                && contentSpan[0] == '<'
                && contentSpan[1] == '@')
            {
                var closingBracketIndex = contentSpan.IndexOf('>');
                if (closingBracketIndex != -1)
                {
                    var idSpan = contentSpan[2] == '!'
                        ? contentSpan.Slice(3, closingBracketIndex - 3)
                        : contentSpan.Slice(2, closingBracketIndex - 2);
                    if (Snowflake.TryParse(idSpan, out var id) && id == message.Client.CurrentUser.Id)
                    {
                        mentionPrefix = contentSpan[2] == '!' ? new StringPrefix($"<@!{id}>") : new StringPrefix($"<@{id}>");
                        return true;
                    }
                }
            }

            mentionPrefix = null;
            return false;
        }
    }
}