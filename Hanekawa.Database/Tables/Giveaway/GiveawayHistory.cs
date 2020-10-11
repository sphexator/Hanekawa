using System;
using Hanekawa.Shared;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class GiveawayHistory
    {
        public Guid Id { get; set; }
        public int IdNum { get; set; }
        public ulong GuildId { get; set; }
        public ulong Winner { get; set; }
        public string Description { get; set; }
        public GiveawayType Type { get; set; }
        public DateTimeOffset CreatedAtOffset { get; set; }
        public DateTimeOffset ClosedAtOffset { get; set; }
    }
}
