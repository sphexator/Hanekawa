using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hanekawa.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Leaderboard
    {
        [JsonProperty("users")] public List<LeaderboardUser> Users { get; set; } = new();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LeaderboardUser
    {
        [JsonProperty("userId")]
        public ulong UserId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("experience")]
        public int Experience { get; set; }

        [JsonProperty("expToLevel")]
        public int ExpToLevel { get; set; }

        [JsonProperty("totalExp")]
        public int TotalExp { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LeaderboardWeekly
    {
        [JsonProperty("users")]
        public List<LeaderboardWeeklyUser> Users { get; set; } = new();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LeaderboardWeeklyUser
    {
        [JsonProperty("userId")]
        public ulong UserId { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LeaderboardRichest
    {
        [JsonProperty("users")]
        public List<LeaderboardRichestUser> Users { get; set; } = new();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LeaderboardRichestUser
    {
        [JsonProperty("userId")]
        public ulong UserId { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }
    }
}
