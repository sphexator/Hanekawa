using Discord;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System.Text;

namespace Hanekawa.Extensions.Embed
{
    public static class PaginatorExtension
    {
        public static PaginatedMessage PaginateBuilder(this List<string> pages, ulong guildId, IGuild guild, string name) =>
            PaginatedMessageBuilder(pages, guildId, guild.IconUrl, name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, ulong guildId, IGuildUser user, string name) =>
            PaginatedMessageBuilder(pages, guildId, user.GetAvatar(), name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, ulong guildId, string name) =>
            PaginatedMessageBuilder(pages, guildId, null, name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, ulong guildId, IGuild guild, string name, int count) =>
            PaginatedMessageBuilder(pages, guildId, guild.IconUrl, name, count);


        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, ulong guildId, string icon,
            string name) => new PaginatedMessage().Builder(pages, guildId, icon, name, 5);

        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, ulong guildId, string icon,
            string name, int count) => new PaginatedMessage().Builder(pages, guildId, icon, name, count);

        private static PaginatedMessage Builder(this PaginatedMessage paginated, IReadOnlyList<string> pages, ulong guildId, string icon, string name, int count)
        {
            paginated.Color = new Color().GetDefaultColor(guildId);
            paginated.Pages = PageBuilder(pages, count);
            paginated.Author = new EmbedAuthorBuilder {IconUrl = icon, Name = name};
            paginated.Options = new PaginatedAppearanceOptions
            {
                First = new Emoji("⏮"),
                Back = new Emoji("◀"),
                Next = new Emoji("▶"),
                Last = new Emoji("⏭"),
                Stop = null,
                Jump = null,
                Info = null
            };
            return paginated;
        }

        private static IEnumerable<string> PageBuilder(this IReadOnlyList<string> list, int count)
        {
            var pages = new List<string>();
            for (var i = 0; i < list.Count;)
            {
                var page = new StringBuilder();
                for (var j = 0; j < count; j++)
                {
                    if (i >= list.Count) continue;
                    var index = list[i];
                    page.AppendLine($"{index}");
                    i++;
                }

                pages.Add(page.ToString());
            }

            return pages;
        }
    }
}
