using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Disqord;
using Disqord.Bot.Prefixes;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Caching
{
    public class CacheService : INService
    {
        public ConcurrentDictionary<Snowflake, MemoryCache> BanCache = new();
        private readonly ConcurrentDictionary<Snowflake, HashSet<IPrefix>> _prefixCollection = new ();

        public HashSet<IPrefix> GetCollection(Snowflake snowflake) => 
            _prefixCollection.TryGetValue(snowflake, out var prefixes) 
                ? prefixes 
                : null;

        public void AddorUpdatePrefix(CachedGuild guild, string prefix)
        {
            var prefixes = new HashSet<IPrefix>() {new StringPrefix(prefix)};
            _prefixCollection.AddOrUpdate(guild.Id, prefixes, (snowflake, set) => prefixes);
        }
        
        public bool IsMentionPrefix(CachedUserMessage message, out IPrefix mentionPrefix)
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
                        var collection = _prefixCollection.GetOrAdd(message.Guild.Id, new HashSet<IPrefix>());
                        collection.Add(mentionPrefix);
                        _prefixCollection.AddOrUpdate(message.Guild.Id, new HashSet<IPrefix>(),
                            (snowflake, set) => collection);
                        return true;
                    }
                }
            }

            mentionPrefix = null;
            return false;
        }
    }
}
