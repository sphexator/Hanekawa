using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;

namespace Hanekawa.Extensions
{
    public static class MessageExtension
    {
        public static async Task<List<Snowflake>> FilterMessagesAsync(this CachedTextChannel channel, int amount = 100,
            CachedMember filterBy = null)
        {
            var messages = await channel.FetchMessagesAsync(amount);
            return messages.ToList().FilterMessages(filterBy);
        }

        public static List<Snowflake> FilterMessages(this List<IMessage> msgs, CachedMember filterBy = null)
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
    }
}