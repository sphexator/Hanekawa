using Newtonsoft.Json;

namespace Hanekawa.Addons.Anilist.Internal
{
    internal class AnilistImage
    {
        [JsonProperty("large")] internal string Large;

        [JsonProperty("medium")] internal string Medium;
    }
}