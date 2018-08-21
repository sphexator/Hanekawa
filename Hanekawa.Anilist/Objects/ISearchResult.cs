using System.Collections.Generic;
using Hanekawa.Anilist.Internal.Queries;

namespace Hanekawa.Anilist.Objects
{
    public interface ISearchResult<T>
    {
        PageInfo PageInfo { get; }
        List<T> Items { get; }
    }
}