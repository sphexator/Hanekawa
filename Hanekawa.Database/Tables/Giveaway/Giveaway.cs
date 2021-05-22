using System;
using System.Collections.Generic;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Giveaway
{
    public class Giveaway
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int IdNum { get; set; } // Number, incremented by number of giveaways in specific guilds
        public Snowflake GuildId { get; set; } // Guild
        public Snowflake Creator { get; set; } // Who created
        public bool Active { get; set; } // Is giveaway active
        public bool Stack { get; set; } = true; // Entries stack?
        public int WinnerAmount { get; set; } = 1;
        public Snowflake? ReactionMessage { get; set; } = null;

        // Requirements
        public int? LevelRequirement { get; set; } = null;
        public TimeSpan? AccountAgeRequirement { get; set; } = null;
        public TimeSpan? ServerAgeRequirement { get; set; } = null;

        // Description of giveaway
        public string Name { get; set; } = "Giveaway";
        public string Description { get; set; } = "Giveaway for this server";
        public GiveawayType Type { get; set; }
        
        // Timestamps
        public DateTimeOffset CreatedAtOffset { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CloseAtOffset { get; set; } = null;

        public virtual List<GiveawayParticipant> Participants { get; set; }
    }
}