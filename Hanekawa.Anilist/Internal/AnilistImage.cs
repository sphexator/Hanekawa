using Newtonsoft.Json;

namespace Hanekawa.Anilist.Internal
{
    internal class AnilistImage
    {
        [JsonProperty("large")]
        internal string Large;

        [JsonProperty("medium")]
        internal string Medium;
    }
}
