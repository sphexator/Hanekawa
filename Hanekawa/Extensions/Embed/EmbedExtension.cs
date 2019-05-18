using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Hanekawa.Core;
using Hanekawa.Database;

namespace Hanekawa.Extensions.Embed
{
    public static class EmbedExtension
    {
        // Reply from channel
        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content, uint color) =>
            channel.SendEmbedAsync(new EmbedBuilder().Create(content, new Color(color)));
        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content, ulong guildId) =>
            channel.SendEmbedAsync(new EmbedBuilder().Create(content, new Color().GetDefaultColor(guildId)));
        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, EmbedBuilder embed) =>
            channel.SendEmbedAsync(embed);

        // Reply from command context
        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, string content, uint color) =>
            context.Channel.SendEmbedAsync(new EmbedBuilder().Create(content, new Color(color)));
        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, string content) =>
            context.Channel.SendEmbedAsync(new EmbedBuilder().Create(content, new Color().GetDefaultColor(context.Guild.Id)));
        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, EmbedBuilder embed) =>
            context.Channel.SendEmbedAsync(embed);

        // Create default embed - used outside of this class
        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content, uint color) =>
            context.Create(content, new Color(color));

        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content, ulong guild) =>
            context.Create(content, new Color().GetDefaultColor(guild));

        // Creates default embed - used here
        private static EmbedBuilder Create(this EmbedBuilder embed, string content, Color color)
        {
            embed.Color = color;
            embed.Description = content;
            return embed;
        }

        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder embed) =>
            channel.SendMessageAsync(null, false, embed.Build());

        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder embed,
            string content) =>
            channel.SendMessageAsync(content, false, embed.Build());
    }
}
