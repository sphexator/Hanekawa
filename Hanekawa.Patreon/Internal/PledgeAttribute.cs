using System;
using Newtonsoft.Json;

namespace Hanekawa.Patreon
{
    public class PledgeAttribute
    {
        [JsonProperty("amount_cents")] public int AmountCents { get; set; }

        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

        [JsonProperty("pledge_cap_cents")] public int? PatronPledgeCapCents { get; set; }

        [JsonProperty("patron_pays_fees")] public bool PatreonPaidFeed { get; set; }

        [JsonProperty("declined_since")] public DateTime? DeclinedSince { get; set; }
    }
}