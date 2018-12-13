using Discord;
using Discord.Commands;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Extensions.Embed
{
    public static class EmbedExtension
    {        
        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content, uint color) =>
            channel.SendEmbedAsync(Create(content, new Color(color)));

        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content, ulong guild) =>
            channel.SendEmbedAsync(Create(content, new Color().GetDefaultColor(guild)));

        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, EmbedBuilder embed) =>
            channel.SendEmbedAsync(embed);

        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, EmbedBuilder embed, string message) =>
            channel.SendEmbedAsync(embed, message);

        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext context, string content, uint color) =>
            context.Channel.SendEmbedAsync(Create(content, new Color(color)));

        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext context, string embedMsg, string message, uint color) =>
            context.Channel.SendEmbedAsync(Create(embedMsg, new Color(color)), message);

        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext context, string content) =>
            context.Channel.SendEmbedAsync(Create(content, new Color().GetDefaultColor(context.Guild.Id)));

        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext context, EmbedBuilder embed) =>
            context.Channel.SendEmbedAsync(embed);

        public static Task<IUserMessage> ReplyAsync(this SocketCommandContext context, EmbedBuilder embed, string message) =>
            context.Channel.SendEmbedAsync(embed, message);

        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content, uint color) =>
            context.Create(content, new Color(color));

        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content, ulong guild) =>
            context.Create(content, new Color().GetDefaultColor(guild));

        public static EmbedBuilder CreateDefault(this EmbedBuilder context, ulong guild) =>
            context.Create(null, new Color().GetDefaultColor(guild));

        public static EmbedBuilder Log(this EmbedBuilder embed, IGuildUser user, string content, uint color,
            string title)
        {
            embed.Author = new EmbedAuthorBuilder {Name = title};
            embed.Description = content;
            embed.Color = new Color(color);
            embed.Timestamp = DateTimeOffset.UtcNow;
            embed.Footer = new EmbedFooterBuilder {IconUrl = user.GetAvatar(), Text = $"ID: {user.Id}"};
            return embed;
        }

        public static List<EmbedFieldBuilder> ModLogFieldBuilders(this List<EmbedFieldBuilder> result,
            IMentionable user,
            string reason,
            TimeSpan? duration) => result.ModLogBuilder(user, reason, duration);

        public static List<EmbedFieldBuilder> ModLogFieldBuilders(this List<EmbedFieldBuilder> result,
            IMentionable user,
            string reason) => result.ModLogBuilder(user, reason);

        public static List<EmbedFieldBuilder> ModLogFieldBuilders(this List<EmbedFieldBuilder> result,
            IMentionable user) => result.ModLogBuilder(user, null);

        private static List<EmbedFieldBuilder> ModLogBuilder(this List<EmbedFieldBuilder> result, IMentionable user,
            string reason, TimeSpan? duration = null)
        {
            result.Add(new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention});
            result.Add(new EmbedFieldBuilder {IsInline = true, Name = "Moderator", Value = "N/A"});
            result.Add(new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason ?? "N/A"});
            if (duration == null) return result;
            var durationField = new EmbedFieldBuilder
            {
                Name = "Duration",
                Value = duration.Value.Humanize(),
                IsInline = true
            };
            result.Add(durationField);
            return result;
        }

        private static EmbedBuilder Create(this EmbedBuilder embed, string content, Color color)
        {
            embed.Color = color;
            embed.Description = content;
            return embed;
        }

        private static EmbedBuilder Create(string content, Color color)
        {
            return new EmbedBuilder
            {
                Color = color,
                Description = content
            };
        }

        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder embed)
            => channel.SendMessageAsync(null, false, embed.Build());

        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel ch, EmbedBuilder embed,
            string content) => ch.SendMessageAsync(content, false, embed.Build());
    }
}