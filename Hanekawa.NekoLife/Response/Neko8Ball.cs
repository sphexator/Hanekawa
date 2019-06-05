using Newtonsoft.Json;

namespace Hanekawa.NekoLife.Response
{
    public class Neko8Ball
    {
        [JsonProperty("response")] public string Response;
        [JsonProperty("url")] public string Url;
    }
}
