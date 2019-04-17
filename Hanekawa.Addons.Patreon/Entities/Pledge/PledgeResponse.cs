using System;

namespace Hanekawa.Patreon.Entities.Pledge
{
    public class PledgeResponse
    {
        public Datum[] data { get; set; }
        public Included[] included { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
    }

    public class Links
    {
        public string first { get; set; }
        public string next { get; set; }
    }

    public class Meta
    {
        public int count { get; set; }
    }

    public class Datum
    {
        public Attributes attributes { get; set; }
        public string id { get; set; }
        public Relationships relationships { get; set; }
        public string type { get; set; }
    }

    public class Attributes
    {
        public int amount_cents { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? declined_since { get; set; }
        public bool patron_pays_fees { get; set; }
        public int pledge_cap_cents { get; set; }
    }

    public class Relationships
    {
        public Patron patron { get; set; }
    }

    public class Patron
    {
        public Data data { get; set; }
        public Links1 links { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Links1
    {
        public string related { get; set; }
    }

    public class Included
    {
        public Attributes1 attributes { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Attributes1
    {
        public string about { get; set; }
        public DateTime created { get; set; }
        public string default_country_code { get; set; }
        public string email { get; set; }
        public object facebook { get; set; }
        public string first_name { get; set; }
        public string full_name { get; set; }
        public int gender { get; set; }
        public string image_url { get; set; }
        public bool is_email_verified { get; set; }
        public string last_name { get; set; }
        public Social_Connections social_connections { get; set; }
        public string thumb_url { get; set; }
        public string twitch { get; set; }
        public object twitter { get; set; }
        public string url { get; set; }
        public string vanity { get; set; }
        public string youtube { get; set; }
    }

    public class Social_Connections
    {
        public object deviantart { get; set; }
        public Discord discord { get; set; }
        public object facebook { get; set; }
        public object reddit { get; set; }
        public object spotify { get; set; }
        public object twitch { get; set; }
        public object twitter { get; set; }
        public object youtube { get; set; }
    }

    public class Discord
    {
        public object url { get; set; }
        public string user_id { get; set; }
    }
}