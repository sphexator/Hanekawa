using Hanekawa.Anilist.Internal.Queries;
using System.Collections.Generic;

namespace Hanekawa.Anilist.Objects
{
    public interface ISearchResult<T>
    {
        PageInfo PageInfo { get; }
        List<T> Items { get; }
    }
}
