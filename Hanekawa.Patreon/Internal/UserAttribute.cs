using System;
using Newtonsoft.Json;

namespace Hanekawa.Patreon
{
    public class UserAttribute
    {
        [JsonProperty("discord_id")] public string DiscordUserId;

        [JsonProperty("about")] public string About { get; set; }

        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

        [JsonProperty("email")] public string EmailAddress { get; set; }

        [JsonProperty("facebook")] public string FacebookUrl { get; set; }

        [JsonProperty("first_name")] public string FirstName { get; set; }

        [JsonProperty("full_name")] public string FullName { get; set; }

        [JsonProperty("gender")] public int Gender { get; set; }

        [JsonProperty("image_url")] public string ImageUrl { get; set; }

        [JsonProperty("is_email_verified")] public bool IsEmailVerified { get; set; }

        [JsonProperty("last_name")] public string LastName { get; set; }

        //[JsonProperty("social_connections")]
        //public Dictionary<string, Dictionary<string, string>> SocialConnections { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        [JsonProperty("thumb_url")] public string ThumbnailUrl { get; set; }

        [JsonProperty("twitch")] public string TwitchUrl { get; set; }

        [JsonProperty("twitter")] public string TwitterUrl { get; set; }

        [JsonProperty("url")] public string PatreonUrl { get; set; }

        [JsonProperty("vanity")] public string Vanity { get; set; }

        [JsonProperty("youtube")] public string YoutubeUrl { get; set; }
    }
}