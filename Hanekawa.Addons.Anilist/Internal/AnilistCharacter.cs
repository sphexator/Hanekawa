using System.Net;
using Hanekawa.Addons.Anilist.Objects;
using Newtonsoft.Json;

namespace Hanekawa.Addons.Anilist.Internal
{
    internal class AnilistCharacter : ICharacter
    {
        [JsonProperty("description")] internal string Description;

        [JsonProperty("id")] internal long Id;

        [JsonProperty("image")] internal AnilistImage Image;

        [JsonProperty("name")] internal AnilistName Name;

        [JsonProperty("siteUrl")] internal string SiteUrl;

        public string FirstName => Name.First;
        public string LastName => Name.Last;
        public string NativeName => Name.Native;

        public string LargeImageUrl => Image?.Large ?? Constants.NoImageUrl;
        public string MediumImageUrl => Image?.Medium ?? Constants.NoImageUrl;

        long ICharacterSearchResult.Id => Id;
        string ICharacter.Description => WebUtility.HtmlDecode(Description);
        string ICharacter.SiteUrl => SiteUrl;
    }
}