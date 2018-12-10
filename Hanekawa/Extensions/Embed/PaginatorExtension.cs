using Discord;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System.Text;

namespace Hanekawa.Extensions.Embed
{
    public static class PaginatorExtension
    {
        public static PaginatedMessage PaginateBuilder(this List<string> pages, IGuild guild, string name) =>
            PaginatedMessageBuilder(pages, guild.IconUrl, name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, IGuildUser user, string name) =>
            PaginatedMessageBuilder(pages, user.GetAvatar(), name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, string name) =>
            PaginatedMessageBuilder(pages, null, name);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, IGuild guild, string name, int count) =>
            PaginatedMessageBuilder(pages, guild.IconUrl, name, count);


        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, string icon,
            string name) => new PaginatedMessage().Builder(pages, icon, name, 5);

        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, string icon,
            string name, int count) => new PaginatedMessage().Builder(pages, icon, name, count);

        private static PaginatedMessage Builder(this PaginatedMessage paginated, IReadOnlyList<string> pages, string icon, string name, int count)
        {
            paginated.Color = Color.Purple;
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
                    var index = list[i];
                    page.Append($"{index}\n");
                    i++;
                }

                pages.Add(page.ToString());
            }

            return pages;
        }
    }
}
