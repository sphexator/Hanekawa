using Newtonsoft.Json;

namespace Hanekawa.Addons.Anilist.Internal
{
    internal class AnilistTitle
    {
        [JsonProperty("english")] internal string english;

        [JsonProperty("native")] internal string native;

        [JsonProperty("romaji")] internal string romaji;

        [JsonProperty("userPreferred")] internal string userPreferred;
    }
}