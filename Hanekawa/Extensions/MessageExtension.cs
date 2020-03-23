using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Hanekawa.Extensions
{
    public static class MessageExtension
    {
        public static async Task<List<Snowflake>> FilterMessagesAsync(this ICachedMessageChannel channel, int amount = 100,
            CachedMember filterBy = null)
        {
            var messages = await channel.GetMessagesAsync(amount);
            return messages.ToList().FilterMessages(filterBy);
        }

        public static List<Snowflake> FilterMessages(this List<RestMessage> msgs, CachedMember filterBy = null)
        {
            var result = new List<Snowflake>();
            for (var i = 0; i < msgs.Count; i++)
            {
                var x = msgs[i];
                // Checks if message can be deleted
                // Messages that's older then 14 days or 2 weeks can't be bulk deleted
                if (x.CreatedAt.AddDays(14) >= DateTimeOffset.UtcNow)
                {
                    // If we're filtering, don't add if its not from the filtered user.
                    if (filterBy != null && x.Author.Id != filterBy.Id) continue;
                    result.Add(x.Id);
                }
            }

            return result;
        }

        public static async Task<bool> TryDeleteMessagesAsync(this CachedTextChannel channel,
            IEnumerable<Snowflake> msgs)
        {
            var currentUser = channel.Guild.CurrentMember;
            if (!currentUser.Permissions.ManageMessages) return false;
            await channel.DeleteMessagesAsync(msgs);
            return true;
        }

        public static async Task<bool> TryDeleteMessageAsync(this CachedTextChannel channel, Snowflake msg)
        {
            var currentUser = channel.Guild.CurrentMember;
            if (!currentUser.Permissions.ManageMessages) return false;
            if (msg.CreatedAt.AddDays(14) <= DateTimeOffset.UtcNow) return false;
            await channel.DeleteMessageAsync(msg);
            return true;
        }

        public static async Task<bool> TryDeleteMessagesAsync(this CachedMessage msg)
        {
            if (!(msg.Channel is CachedTextChannel chn)) return false;
            if (!chn.Guild.CurrentMember.Permissions.ManageMessages) return false;
            if (msg.CreatedAt.AddDays(14) <= DateTimeOffset.UtcNow) return false;
            await msg.DeleteAsync();
            return true;
        }

        public static async Task<bool> TryDeleteMessageAsync(this CachedMessage msg)
        {
            if (!(msg.Channel is CachedTextChannel chn)) return false;
            if (!chn.Guild.CurrentMember.Permissions.ManageMessages) return false;
            try
            {
                await msg.DeleteAsync();
            }
            catch { /* Ignore ? */ }
            return true;
        }
    }
}