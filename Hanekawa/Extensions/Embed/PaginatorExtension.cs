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

        private static PaginatedMessage PaginatedMessageBuilder(this IReadOnlyList<string> pages, string icon,
            string name)
        {
            return new PaginatedMessage
            {
                Color = Color.Purple,
                Pages = PageBuilder(pages),
                Author = new EmbedAuthorBuilder { IconUrl = icon, Name = name },
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
                var page = new StringBuilder();
                for (var j = 0; j < 5; j++)
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
