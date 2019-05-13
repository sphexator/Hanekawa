using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class MessageExtensions
    {
        public static async Task<bool> TryDeleteMessagesAsync(this SocketTextChannel channel,
            IEnumerable<IMessage> msgs)
        {
            var currentUser = channel.Guild.CurrentUser;
            if (currentUser.GuildPermissions.ManageMessages)
            {
                await channel.DeleteMessagesAsync(msgs);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryDeleteMessageAsync(this SocketTextChannel channel, IMessage msg)
        {
            var currentUser = channel.Guild.CurrentUser;
            if (currentUser.GuildPermissions.ManageMessages)
            {
                await channel.DeleteMessageAsync(msg);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryDeleteMessagesAsync(this SocketMessage msg)
        {
            if (!(msg.Channel is SocketTextChannel chn)) return false;
            if (!chn.Guild.CurrentUser.GuildPermissions.ManageMessages) return false;
            await msg.DeleteAsync();
            return true;
        }

        public static async Task<bool> TryDeleteMessageAsync(this SocketUserMessage msg)
        {
            if (!(msg.Channel is SocketTextChannel chn)) return false;
            if (!chn.Guild.CurrentUser.GuildPermissions.ManageMessages) return false;
            await msg.DeleteAsync();
            return true;
        }
    }
}
