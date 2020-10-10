using System;
using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class Giveaway
    {
        public Guid Id { get; set; }
        public int IdNum { get; set; }
        public ulong GuildId { get; set; }
        public string Description { get; set; }
        // TODO: Add type of giveaway, ie. Draw, Vote, Activity
        public DateTimeOffset CreatedAtOffset { get; set; }
        public ulong Creator { get; set; }

        public virtual List<GiveawayParticipant> Participants { get; set; }
    }
}