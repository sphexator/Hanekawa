using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class MessageExtension
    {
        public static async Task<List<IMessage>> FilterMessagesAsync(this SocketTextChannel channel, int amount = 100,
            SocketGuildUser filterBy = null)
        {
            var messages = await channel.GetMessagesAsync(amount).FlattenAsync();
            return messages.ToList().FilterMessages(filterBy);
        }
        
        public static List<IMessage> FilterMessages(this List<IMessage> msgs, SocketGuildUser filterBy = null)
        {
            var result = new List<IMessage>();
            for (int i = 0; i < msgs.Count; i++)
            {
                var x = msgs[i];
                // Checks if message can be deleted
                // Messages that's older then 14 days or 2 weeks can't be bulk deleted
                if (x.Timestamp.AddDays(13) < DateTimeOffset.UtcNow)
                {
                    // If we're filtering, don't add if its not from the filtered user.
                    if (filterBy != null && x.Author == filterBy) result.Add(x);
                    else result.Add(x);
                }
            }

            return result;
        }

        public static async Task<bool> TryDeleteMessagesAsync(this SocketTextChannel channel, 
            IEnumerable<IMessage> msgs)
        {
            var currentUser = channel.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageMessages) return false;
            await channel.DeleteMessagesAsync(msgs);
            return true;

        }

        public static async Task<bool> TryDeleteMessageAsync(this SocketTextChannel channel, IMessage msg)
        {
            var currentUser = channel.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageMessages) return false;
            await channel.DeleteMessageAsync(msg);
            return true;

        }

        public static async Task<bool> TryDeleteMessagesAsync(this IMessage msg)
        {
            if (!(msg.Channel is SocketTextChannel chn)) return false;
            if (!chn.Guild.CurrentUser.GuildPermissions.ManageMessages) return false;
            await msg.DeleteAsync();
            return true;
        }

        public static async Task<bool> TryDeleteMessageAsync(this IMessage msg)
        {
            if (!(msg.Channel is SocketTextChannel chn)) return false;
            if (!chn.Guild.CurrentUser.GuildPermissions.ManageMessages) return false;
            await msg.DeleteAsync();
            return true;
        }
    }
}