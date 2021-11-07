using Newtonsoft.Json;

namespace Hanekawa.WebUI.Models
{
    public class DslWebhook
    {
        [JsonProperty("guild")]
        public string Guild { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }
    }
}