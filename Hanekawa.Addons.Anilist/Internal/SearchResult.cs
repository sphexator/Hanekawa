using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hanekawa.Addons.Anilist.Internal.Queries;
using Hanekawa.Addons.Anilist.Objects;

namespace Hanekawa.Addons.Anilist.Internal
{
    internal class SearchResult<T> : ISearchResult<T>
    {
        internal SearchResult(BasePage q)
        {
            PageInfo = q.PageInfo;

            var fields = q.GetType()
                .GetRuntimeFields();

            var f = fields
                .FirstOrDefault(x => x.FieldType == Items.GetType());

            Items = f.GetValue(q) as List<T>;
        }

        internal SearchResult(PageInfo info, List<T> list)
        {
            PageInfo = info;
            Items = list;
        }

        public PageInfo PageInfo { get; }

        public List<T> Items { get; } = new List<T>();

        internal ISearchResult<U> ToInterface<U>()
        {
            return new SearchResult<U>(PageInfo, Items.Cast<U>().ToList());
        }
    }
}