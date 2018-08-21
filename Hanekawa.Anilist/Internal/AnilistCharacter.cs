﻿using Newtonsoft.Json;
using System.Net;
using Hanekawa.Anilist.Objects;

namespace Hanekawa.Anilist.Internal
{
    internal class AnilistCharacter : ICharacter
    {
        public string FirstName => Name.First;
        public string LastName => Name.Last;
        public string NativeName => Name.Native;

        public string LargeImageUrl => Image?.Large ?? Constants.NoImageUrl;
        public string MediumImageUrl => Image?.Medium ?? Constants.NoImageUrl;

        long ICharacterSearchResult.Id => Id;
        string ICharacter.Description => WebUtility.HtmlDecode(Description);
        string ICharacter.SiteUrl => SiteUrl;

        [JsonProperty("id")]
        internal long Id;

        [JsonProperty("name")]
        internal AnilistName Name;

        [JsonProperty("description")]
        internal string Description;

        [JsonProperty("siteUrl")]
        internal string SiteUrl;

        [JsonProperty("image")]
        internal AnilistImage Image;
    }
}
