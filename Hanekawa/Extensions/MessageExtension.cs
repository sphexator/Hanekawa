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
                    if (filterBy != null && x.Author.Id.RawValue != filterBy.Id.RawValue) continue;
                    result.Add(x.Id.RawValue);
                }
            }

            return result;
        }

        public static async Task<bool> TryDeleteMessagesAsync(this ICachedMessageChannel channel,
            IEnumerable<Snowflake> messageIds)
        {
            if (!(channel is CachedTextChannel gChannel)) return true;
            var currentUser = gChannel.Guild.CurrentMember;
            if (!currentUser.Permissions.ManageMessages) return false;
            await gChannel.DeleteMessagesAsync(messageIds);
            return true;
        }

        public static async Task<bool> TryDeleteMessageAsync(this ICachedMessageChannel channel, Snowflake messageId)
        {
            if (!(channel is CachedTextChannel gChannel)) return true;
            var currentUser = gChannel.Guild.CurrentMember;
            if (!currentUser.Permissions.ManageMessages) return false;
            if (messageId.CreatedAt.AddDays(14) <= DateTimeOffset.UtcNow) return false;
            await channel.DeleteMessageAsync(messageId);
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