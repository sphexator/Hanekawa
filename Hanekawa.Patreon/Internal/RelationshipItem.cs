using Newtonsoft.Json;

namespace Hanekawa.Patreon
{
    public class RelationshipItem
    {
        [JsonProperty("data")] public PatreonEntity Data { get; set; }

        [JsonProperty("links")] public PatreonLinks Links { get; set; }
    }
}