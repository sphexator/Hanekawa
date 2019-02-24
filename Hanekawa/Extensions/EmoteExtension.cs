using Discord;

namespace Hanekawa.Extensions
{
    public static class EmoteExtension
    {
        public static string ParseToString(this Emote emote) =>
            emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";
    }
}
