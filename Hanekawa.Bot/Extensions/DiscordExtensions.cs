using Disqord;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Bot.Extensions;

internal static class DiscordExtensions
{
    internal static LocalEmbed ToLocalEmbed(this Embed embed)
    {
        var toReturn = new LocalEmbed
        {
            Author = new LocalEmbedAuthor
            {
                Name = embed.Header.Name,
                IconUrl = embed.Header.IconUrl,
                Url = embed.Header.Url
            },
            Title = embed.Title,
            ThumbnailUrl = embed.Icon,
            Color = new Color(embed.Color),
            Description = embed.Content,
            ImageUrl = embed.Attachment,
            Timestamp = embed.Timestamp,
            Footer = new LocalEmbedFooter
            {
                IconUrl = embed.Footer.IconUrl,
                Text = embed.Footer.Text,
            }
        };
        var fields = new List<LocalEmbedField>();
        for (var i = 0; i < embed.Fields.Count; i++)
        {
            var x = embed.Fields[i];
            fields.Add(new LocalEmbedField{ Name = x.Name, Value = x.Value, IsInline = x.IsInline });
        }

        if (fields.Count != 0) toReturn.Fields = fields;
        return toReturn;
    }
}