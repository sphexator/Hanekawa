using System.Collections.Generic;

namespace Hanekawa.Anilist.Objects
{
    public interface IMediaSearchResult
    {
        int Id { get; }

        string DefaultTitle { get; }

        string EnglishTitle { get; }

        string NativeTitle { get; }
    }

    public interface IMedia : IMediaSearchResult
    {
        int? Chapters { get; }

        string CoverImage { get; }

        string Description { get; }

        int? Duration { get; }

        int? Episodes { get; }

        IReadOnlyList<string> Genres { get; }

        int? Score { get; }

        string Status { get; }

        string Url { get; }

        int? Volumes { get; }
    }
}
