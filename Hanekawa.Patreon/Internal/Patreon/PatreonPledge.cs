using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hanekawa.Patreon
{
    public class PatreonPledge
    {
        [JsonProperty("data")] public PatreonEntity Data { get; set; }

        [JsonProperty("included")] public List<PatreonEntity> Included { get; set; } = new List<PatreonEntity>();

        [JsonProperty("relationships")]
        public Dictionary<string, string> Links { get; set; } = new Dictionary<string, string>();

        //[JsonProperty("meta")]
        //public Dictionary<string, object> Meta { get; set; } = new Dictionary<string, object>();
    }
}