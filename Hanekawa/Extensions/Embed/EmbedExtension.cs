using System;
using Disqord;
using Quartz.Util;

namespace Hanekawa.Extensions.Embed
{
    public static class EmbedExtension
    {
        // Creates default embed - used here
        public static LocalEmbedBuilder Create(this LocalEmbedBuilder embed, string content, Color colour)
        {
            embed.Color = colour;
            embed.Description = content;
            return embed;
        }

        public static LocalEmbedBuilder ToEmbedBuilder(this LocalEmbed embed)
        {
            var newEmbed = new LocalEmbedBuilder
            {
                Description = embed.Description,
                Color = embed.Color,
                Title = embed.Title,
                Timestamp = embed.Timestamp,
                Url = embed.Url
            };

            if (embed.Author.Name != null)
                newEmbed.Author = new LocalEmbedAuthorBuilder
                    {IconUrl = embed.Author.IconUrl, Name = embed.Author.Name, Url = embed.Author.Url};
            if (embed.Footer.Text != null)
                newEmbed.Footer = new LocalEmbedFooterBuilder
                    {IconUrl = embed.Footer.IconUrl, Text = embed.Footer.IconUrl};

            if (embed.ImageUrl != null) newEmbed.ImageUrl = embed.ImageUrl;
            if (embed.ThumbnailUrl != null) newEmbed.ThumbnailUrl = embed.ThumbnailUrl;
            foreach (var x in embed.Fields) newEmbed.AddField(x.Name, x.Value, x.IsInline);

            return newEmbed;
        }

        public static LocalEmbedBuilder ToEmbedBuilder(this Disqord.Embed embed)
        {
            var newEmbed = new LocalEmbedBuilder
            {
                Description = embed.Description,
                Color = embed.Color,
                Title = embed.Title,
                Timestamp = embed.Timestamp,
                Url = embed.Url
            };

            if (!embed.Author.Name.IsNullOrWhiteSpace())
                newEmbed.Author = new LocalEmbedAuthorBuilder
                    { IconUrl = embed.Author.IconUrl, Name = embed.Author.Name, Url = embed.Author.Url };
            if (!embed.Footer.Text.IsNullOrWhiteSpace())
                newEmbed.Footer = new LocalEmbedFooterBuilder
                    { IconUrl = embed.Footer.IconUrl, Text = embed.Footer.Text };
            try
            {
                if (embed.Image.Url != null) newEmbed.ImageUrl = embed.Image.Url;
                if (embed.Thumbnail.Url != null) newEmbed.ThumbnailUrl = embed.Thumbnail.Url;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            foreach (var x in embed.Fields) newEmbed.AddField(x.Name, x.Value, x.IsInline);

            return newEmbed;
        }
    }
}