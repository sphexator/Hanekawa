using System;
using Discord;

namespace Jibril.Services.Common
{
    public class EmbedGenerator
    {
        public static EmbedBuilder DefaultEmbed(string content, uint color)
        {
            var embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            return embed;
        }

        public static EmbedBuilder FooterEmbed(string content, string footcont, uint color, IUser user)
        {
            var footer = new EmbedFooterBuilder
            {
                Text = footcont,
                IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
            };

            var embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content,
                Footer = footer,
                Timestamp = new DateTimeOffset(DateTime.UtcNow)
            };

            return embed;
        }

        public static EmbedBuilder AuthorEmbed(string content, string authContent, uint color, IUser user)
        {
            var embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(user.GetAvatarUrl());
            author.WithName(user.Username);

            embed.WithAuthor(author);

            return embed;
        }

        public static EmbedBuilder FullEmbed(string content, uint color, IUser user)
        {
            var embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(user.GetAvatarUrl());
            author.WithName(user.Username);

            var footer = new EmbedFooterBuilder();
            footer.WithText($"{DateTime.Now}");

            embed.WithAuthor(author);
            embed.WithFooter(footer);

            return embed;
        }
    }
}