using System;
using System.Threading.Tasks;
using Discord;

namespace Hanekawa.Extensions
{
    public static class EmbedExtension 
    {
        public static Task<IUserMessage> SendEmbedAsync(this IMessageChannel ch, EmbedBuilder getEmbed, string content = null)
            => ch.SendMessageAsync(content, embed: getEmbed.Build());

        public static EmbedBuilder Reply(this EmbedBuilder embed, string content, uint color = 0)
        {
            if (color == 0) color = Color.Purple.RawValue;
            embed.Description = content;
            embed.Color = new Color(color);
            return embed;
        }

        public static EmbedBuilder Log(this EmbedBuilder embed, IGuildUser user, string content, uint color, bool ban = false, string title = null)
        {
            var footer = new EmbedFooterBuilder
            {
                IconUrl = user.GetAvatar(),
                Text = $"ID: {user.Id}"
            };
            embed.Description = content;
            embed.Color = new Color(color);
            embed.Footer = footer;
            embed.Timestamp = DateTimeOffset.UtcNow;
            if (!ban) return embed;
            var author = new EmbedAuthorBuilder
            {
                Name = title
            };
            embed.Author = author;
            return embed;
        }
    }
}