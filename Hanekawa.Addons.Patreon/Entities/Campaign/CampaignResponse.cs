using System;

namespace Hanekawa.Addons.Patreon.Entities.Campaign
{
    public class CampaignResponse
    {
        public Datum[] data { get; set; }
        public Included[] included { get; set; }
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
        public string avatar_photo_url { get; set; }
        public string cover_photo_url { get; set; }
        public DateTime created_at { get; set; }
        public int creation_count { get; set; }
        public string creation_name { get; set; }
        public string discord_server_id { get; set; }
        public bool display_patron_goals { get; set; }
        public string earnings_visibility { get; set; }
        public string image_small_url { get; set; }
        public string image_url { get; set; }
        public bool is_charge_upfront { get; set; }
        public bool is_charged_immediately { get; set; }
        public bool is_monthly { get; set; }
        public bool is_nsfw { get; set; }
        public bool is_plural { get; set; }
        public object main_video_embed { get; set; }
        public object main_video_url { get; set; }
        public object one_liner { get; set; }
        public int outstanding_payment_amount_cents { get; set; }
        public int patron_count { get; set; }
        public string pay_per_name { get; set; }
        public int pledge_sum { get; set; }
        public string pledge_url { get; set; }
        public DateTime published_at { get; set; }
        public string summary { get; set; }
        public object thanks_embed { get; set; }
        public string thanks_msg { get; set; }
        public object thanks_video_url { get; set; }
    }

    public class Relationships
    {
        public Creator creator { get; set; }
        public Goals goals { get; set; }
        public Rewards rewards { get; set; }
    }

    public class Creator
    {
        public Data data { get; set; }
        public Links links { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Links
    {
        public string related { get; set; }
    }

    public class Goals
    {
        public object[] data { get; set; }
    }

    public class Rewards
    {
        public Datum1[] data { get; set; }
    }

    public class Datum1
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Included
    {
        public Attributes1 attributes { get; set; }
        public string id { get; set; }
        public Relationships1 relationships { get; set; }
        public string type { get; set; }
    }

    public class Attributes1
    {
        public object about { get; set; }
        public object can_see_nsfw { get; set; }
        public DateTime created { get; set; }
        public object default_country_code { get; set; }
        public string discord_id { get; set; }
        public string email { get; set; }
        public object facebook { get; set; }
        public object facebook_id { get; set; }
        public string first_name { get; set; }
        public string full_name { get; set; }
        public int gender { get; set; }
        public bool has_password { get; set; }
        public string image_url { get; set; }
        public bool is_deleted { get; set; }
        public bool is_email_verified { get; set; }
        public bool is_nuked { get; set; }
        public bool is_suspended { get; set; }
        public string last_name { get; set; }
        public Social_Connections social_connections { get; set; }
        public string thumb_url { get; set; }
        public object twitch { get; set; }
        public object twitter { get; set; }
        public string url { get; set; }
        public string vanity { get; set; }
        public object youtube { get; set; }
        public int amount { get; set; }
        public int amount_cents { get; set; }
        public DateTime? created_at { get; set; }
        public string description { get; set; }
        public int? remaining { get; set; }
        public bool requires_shipping { get; set; }
        public int? user_limit { get; set; }
        public string[] discord_role_ids { get; set; }
        public DateTime edited_at { get; set; }
        public int patron_count { get; set; }
        public int post_count { get; set; }
        public bool published { get; set; }
        public DateTime published_at { get; set; }
        public string title { get; set; }
        public object unpublished_at { get; set; }
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
        public string[] scopes { get; set; }
        public object url { get; set; }
        public string user_id { get; set; }
    }

    public class Relationships1
    {
        public campaign campaign { get; set; }
    }

    public class campaign
    {
        public Data1 data { get; set; }
        public Links1 links { get; set; }
    }

    public class Data1
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Links1
    {
        public string related { get; set; }
    }
}