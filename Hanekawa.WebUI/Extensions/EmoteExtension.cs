using System;
using Disqord;

namespace Hanekawa.WebUI.Extensions
{
    public static class EmoteExtension
    {
        public static Snowflake? GetId(this IEmoji emote)
        {
            if (emote is ICustomEmoji customEmoji) return customEmoji.Id;
            return null;
        }

        public static Snowflake? GetEmoteId(this string messageEmoteString)
        {
            if (Snowflake.TryParse(messageEmoteString.Replace(">", "")
                    .Replace("<:", "")
                    .Replace("<a:", "")
                    .Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1],
                out var snowflake)) return snowflake;
            return null;
        }

        public static string GetEmoteUrl(this ICustomEmoji emoji) => emoji.IsAnimated
            ? $"https://cdn.discordapp.com/emojis/{emoji.Id}.gif"
            : $"https://cdn.discordapp.com/emojis/{emoji.Id}.png";
    }
}