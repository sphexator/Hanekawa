using System.Collections.Generic;
using System.Net;
using Hanekawa.Anilist.Objects;
using Newtonsoft.Json;

namespace Hanekawa.Anilist.Internal
{
    internal class AnilistMedia : IMedia
    {
        [JsonProperty("chapters")] internal int? chapters;

        [JsonProperty("countryOfOrigin")] internal string countryCode;

        [JsonProperty("coverImage")] internal AnilistImage coverImage;

        [JsonProperty("description")] internal string description;

        [JsonProperty("duration")] internal int? duration;

        [JsonProperty("endDate")] internal int endDate;

        [JsonProperty("episodes")] internal int? episodeCount;

        [JsonProperty("genres")] internal List<string> genres = new List<string>();

        [JsonProperty("hashtag")] internal string hashtag;

        [JsonProperty("id")] internal int id;

        [JsonProperty("isAdult")] internal bool isAdultContent;

        [JsonProperty("isLicensed")] internal bool isLicensed;

        [JsonProperty("idMal")] internal int malId;

        [JsonProperty("format")] internal string mediaFormat;

        [JsonProperty("status")] internal string mediaStatus;

        [JsonProperty("type")] internal string mediaType;

        [JsonProperty("averageScore")] internal int? score;

        [JsonProperty("season")] internal string season;

        [JsonProperty("seasonYear")] internal int seasonYear;

        [JsonProperty("siteUrl")] internal string siteUrl;

        [JsonProperty("source")] internal string source;

        [JsonProperty("startDate")] internal int startDate;

        [JsonProperty("title")] internal AnilistTitle title;

        [JsonProperty("volumes")] internal int? volumes;

        public string CoverImage => coverImage.Large ?? Constants.NoImageUrl;
        public string DefaultTitle => title.userPreferred;

        public string Description => WebUtility.HtmlDecode(description)
            .Replace("<br>", "\n");

        public int? Duration => duration;
        public int? Episodes => episodeCount;
        public int? Volumes => volumes;
        public int? Chapters => chapters;
        public string EnglishTitle => title.english;
        public IReadOnlyList<string> Genres => genres;
        public int Id => id;
        public string NativeTitle => title.native;
        public int? Score => score;
        public string Status => mediaStatus;
        public string Url => siteUrl;
    }
}