using System;
using Disqord.Serialization.Json;
using Hanekawa.Database.Tables.Moderation;

namespace Hanekawa.Models.Api
{
    public class BanCase
    {
        public BanCase() { }

        public BanCase(ModLog modLog)
        {
            GuildId = modLog.GuildId;
            UserId = modLog.UserId;
            ModeratorId = modLog.ModId;
            Date = modLog.Date;
            Reason = modLog.Response;
        }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("userId")]
        public ulong UserId { get; set; }

        [JsonProperty("moderatorId")]
        public ulong? ModeratorId { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
