using System;

namespace Hanekawa.Entities.Giveaway
{
    public class GiveawayParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset Entry { get; set; } = DateTimeOffset.UtcNow;

        public Guid GiveawayId { get; set; }
        public virtual Giveaway Giveaway { get; set; }
    }
}