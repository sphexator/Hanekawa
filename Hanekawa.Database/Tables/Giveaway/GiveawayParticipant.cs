using System;
using Disqord;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class GiveawayParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public DateTimeOffset Entry { get; set; } = DateTimeOffset.UtcNow;

        public Guid GiveawayId { get; set; }
        public virtual Giveaway Giveaway { get; set; }
    }
}