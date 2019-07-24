using System;
using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService
    {
        private readonly ConcurrentDictionary<ulong, string> _reactionEmote
            = new ConcurrentDictionary<ulong, string>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _reactionMessages
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private int GetReactionAmount(SocketGuild guild, IUserMessage msg)
        {
            var messages = _reactionMessages.GetOrAdd(guild.Id, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id, out var result);
            if (check)
            {
                var amount = (int) result;
                messages.Set(msg.Id, amount, TimeSpan.FromDays(1));
                return amount;
            }

            return 0;
        }

        private void IncreaseReactionAmount(SocketGuild guild, IUserMessage msg)
        {
            var messages = _reactionMessages.GetOrAdd(guild.Id, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id, out var result);
            if (check)
            {
                var amount = (int) result;
                messages.Set(msg.Id, amount + 1, TimeSpan.FromDays(1));
                return;
            }

            messages.Set(msg.Id, 1, TimeSpan.FromDays(1));
        }

        private void DecreaseReactionAmount(SocketGuild guild, IUserMessage msg)
        {
            var messages = _reactionMessages.GetOrAdd(guild.Id, new MemoryCache(new MemoryCacheOptions()));
            var check = messages.TryGetValue(msg.Id, out var result);
            if (!check) return;
            var amount = (int) result;
            if (amount - 1 <= 0)
                messages.Remove(msg.Id);
            else
                messages.Set(msg.Id, amount - 1, TimeSpan.FromDays(1));
        }
    }
}