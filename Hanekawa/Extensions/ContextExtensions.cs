using System;
using System.Collections.Generic;
using System.Text;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Hanekawa.Extensions
{
    public static class ContextExtensions
    {
        public static List<Page> PaginationBuilder(this List<string> list, Color color, string avatarUrl, string authorTitle, int inputPerPage = 5)
        {
            var pages = new List<Page>();
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count;)
            {
                for (var j = 0; j < inputPerPage; j++)
                {
                    if (i >= list.Count) continue;
                    var x = list[i];
                    sb.AppendLine(x);
                    i++;
                }

                var page = pages.Count + 1;
                var maxPage = Convert.ToInt32(Math.Ceiling((double) list.Count / inputPerPage));
                pages.Add(new Page(new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder {Name = authorTitle, IconUrl = avatarUrl},
                    Description = sb.ToString(),
                    Color = color,
                    Footer = new LocalEmbedFooterBuilder {Text = $"Page: {page}/{maxPage}"}
                }));
                sb.Clear();
            }

            return pages;
        }
    }
}