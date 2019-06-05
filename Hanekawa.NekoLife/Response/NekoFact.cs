using Newtonsoft.Json;

namespace Hanekawa.NekoLife.Response
{
    public class NekoFact
    {
        [JsonProperty("fact")] public string Fact;
    }
}
