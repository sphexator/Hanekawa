using System.Collections.Generic;
using Hanekawa.Addons.Anilist.Internal.Queries;

namespace Hanekawa.Addons.Anilist.Objects
{
    public interface ISearchResult<T>
    {
        PageInfo PageInfo { get; }
        List<T> Items { get; }
    }
}