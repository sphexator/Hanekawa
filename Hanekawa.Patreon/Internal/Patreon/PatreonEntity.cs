using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hanekawa.Patreon
{
    public class PatreonEntity
    {
        [JsonProperty("attributes")] public JObject Attributes { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("relationships")] public JObject Relationships { get; set; }

        [JsonProperty("type")] public PatreonType Type { get; set; }
    }
}