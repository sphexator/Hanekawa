using System;
using System.Collections.Concurrent;
using Disqord;
using Microsoft.Extensions.Caching.Memory;

#nullable enable

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService
    {
        private static readonly ConcurrentDictionary<ulong, string> ReactionEmote
            = new ConcurrentDictionary<ulong, string>();

        private static readonly ConcurrentDictionary<ulong, MemoryCache> ReactionMessages
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private static int GetReactionAmount(CachedGuild guild, IMessage msg)
        {
            var messages = ReactionMessages.GetOrAdd(guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id.RawValue, out var result);
            if (check)
            {
                var amount = (int) result;
                messages.Set(msg.Id.RawValue, amount, TimeSpan.FromDays(1));
                return amount;
            }

            return 0;
        }

        private static void IncreaseReactionAmount(CachedGuild guild, IMessage msg)
        {
            var messages = ReactionMessages.GetOrAdd(guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id.RawValue, out var result);
            if (check)
            {
                var amount = (int) result;
                messages.Set(msg.Id.RawValue, amount + 1, TimeSpan.FromDays(1));
                return;
            }

            messages.Set(msg.Id.RawValue, 1, TimeSpan.FromDays(1));
        }

        private static void DecreaseReactionAmount(CachedGuild guild, IMessage msg)
        {
            var messages = ReactionMessages.GetOrAdd(guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id.RawValue, out var result);
            if (!check) return;
            var amount = (int) result;
            if (amount - 1 <= 0)
                messages.Remove(msg.Id.RawValue);
            else
                messages.Set(msg.Id.RawValue, amount - 1, TimeSpan.FromDays(1));
        }
    }
}