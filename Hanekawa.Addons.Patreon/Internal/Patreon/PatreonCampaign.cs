using Hanekawa.Addons.Patreon.Internal.Campaign;
using Newtonsoft.Json;

namespace Hanekawa.Patreon
{
    public class PatreonCampaign : PatreonEntity
    {
        [JsonProperty("attributes")] public CampaignAttribute Attributes { get; set; }
    }
}