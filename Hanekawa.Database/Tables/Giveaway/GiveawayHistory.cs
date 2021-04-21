using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class GiveawayHistory
    {
        public Guid Id { get; set; }
        public int IdNum { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake Creator { get; set; } // Who created
        public Snowflake[] Winner { get; set; }

        public string Name { get; set; } = "Giveaway";
        public string Description { get; set; } = "Giveaway for this server";
        public GiveawayType Type { get; set; }
        
        public DateTimeOffset CreatedAtOffset { get; set; }
        public DateTimeOffset ClosedAtOffset { get; set; }
    }
}