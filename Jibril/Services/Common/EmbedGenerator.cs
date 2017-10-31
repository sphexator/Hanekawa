using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Common
{
    public class EmbedGenerator
    {
        public static EmbedBuilder DefaultEmbed(string content, uint color)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            return embed;
        }

        public static EmbedBuilder FooterEmbed(string content, string footcont, uint color, IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.WithText(footcont);
            footer.WithIconUrl(user.GetAvatarUrl());

            embed.WithFooter(footer);

            return embed;
        }

        public static EmbedBuilder AuthorEmbed(string content, string authContent, uint color, IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            author.WithIconUrl(user.GetAvatarUrl());
            author.WithName(user.Username);

            embed.WithAuthor(author);

            return embed;
        }

        public static EmbedBuilder FullEmbed(string content, uint color, IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(color),
                Description = content
            };

            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            author.WithIconUrl(user.GetAvatarUrl());
            author.WithName(user.Username);

            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.WithText($"{DateTime.Now}");

            embed.WithAuthor(author);
            embed.WithFooter(footer);

            return embed;
        }
    }
}
