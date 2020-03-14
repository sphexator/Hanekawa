﻿using System.Collections.Generic;
using System.Text;

namespace Hanekawa.Extensions.Embed
{
    public static class PaginatorExtension
    {
        /*
        public static PaginatedMessage PaginateBuilder(this List<string> pages, SocketGuild guild, string authorName,
            string title, int count = 5)
            => new PaginatedMessage().Builder(pages, guild.Id, guild.IconUrl, authorName, title, count);

        public static PaginatedMessage PaginateBuilder(this List<string> pages, SocketGuildUser user, string authorName,
            string title, int count = 5)
            => new PaginatedMessage().Builder(pages, user.Guild.Id, user.GetAvatar(), authorName, title, count);

        private static PaginatedMessage Builder(this PaginatedMessage paginated, IReadOnlyList<string> pages,
            ulong guildId, string authorIcon, string authorName, string title, int count)
        {
            paginated.Color = new Color().GetDefaultColor(guildId);
            paginated.Pages = PageBuilder(pages, count);
            paginated.Author = new EmbedAuthorBuilder {IconUrl = authorIcon, Name = authorName};
            paginated.Title = title;
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
                    page.AppendLine(index);
                    i++;
                }

                pages.Add(page.ToString());
            }

            return pages;
        }
        */
    }
}