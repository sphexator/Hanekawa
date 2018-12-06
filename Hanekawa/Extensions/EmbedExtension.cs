using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Humanizer;

namespace Hanekawa.Extensions
{
    public static class EmbedExtension
    {
        public static Task<IUserMessage> SendEmbedAsync(this IMessageChannel ch, EmbedBuilder getEmbed,
            string content = null) => ch.SendMessageAsync(content, embed: getEmbed.Build());

        public static Task<IUserMessage> Reply(this EmbedBuilder embed, IMessageChannel ch, string content, uint color = 0)
        {
            if (color == 0) color = Color.Purple.RawValue;
            embed.Description = content;
            embed.Color = new Color(color);
            return ch.SendMessageAsync(null, embed: embed.Build());
        }

        public static EmbedBuilder Reply(this EmbedBuilder embed, string content, uint color = 0)
        {
            if (color == 0) color = Color.Purple.RawValue;
            embed.Description = content;
            embed.Color = new Color(color);
            return embed;
        }

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

        public static PaginatedMessage PaginateBuilder(this List<string> pages, IGuild guild, string name) =>
            PaginatedMessageBuilder(pages, guild.IconUrl, name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, IGuildUser user, string name) =>
            PaginatedMessageBuilder(pages, user.GetAvatar(), name);

        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, string icon,
            string name)
        {
            return new PaginatedMessage
            {
                Color = Color.Purple,
                Pages = PageBuilder(pages),
                Author = new EmbedAuthorBuilder { IconUrl = icon, Name = name},
                Options = new PaginatedAppearanceOptions
                {
                    First = new Emoji("⏮"),
                    Back = new Emoji("◀"),
                    Next = new Emoji("▶"),
                    Last = new Emoji("⏭"),
                    Stop = null,
                    Jump = null,
                    Info = null
                }
            };
        }

        private static IEnumerable<string> PageBuilder(this IReadOnlyList<string> list)
        {
            var pages = new List<string>();
            for (var i = 0; i < list.Count;)
            {
                string page = null;
                for (var j = 0; j < 5; j++)
                {
                    var index = list[i];
                    page += $"{index}\n";
                    i++;
                }
                pages.Add(page);
            }
            return pages;
        }

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
    }
}