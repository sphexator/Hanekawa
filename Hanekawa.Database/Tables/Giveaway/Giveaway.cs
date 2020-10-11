using System;
using System.Collections.Generic;
using Hanekawa.Shared;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class Giveaway
    {
        public Guid Id { get; set; }
        public int IdNum { get; set; }
        public ulong GuildId { get; set; }
        public string Description { get; set; }
        public GiveawayType Type { get; set; }
        public DateTimeOffset CreatedAtOffset { get; set; }
        public DateTimeOffset? CloseAtOffset { get; set; }
        public ulong Creator { get; set; }
        public bool Active { get; set; }
        public bool Stack { get; set; }

        public virtual List<GiveawayParticipant> Participants { get; set; }
    }
}