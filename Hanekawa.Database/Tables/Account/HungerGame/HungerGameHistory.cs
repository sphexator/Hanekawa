using System;
using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameHistory
    {
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; } = DateTimeOffset.UtcNow;
        public Snowflake Winner { get; set; }
        public Snowflake[] Participants { get; set; }

        public int Experience { get; set; } = 0;
        public int Credit { get; set; } = 0;
        public int SpecialCredit { get; set; } = 0;
        public Snowflake? Role { get; set; } = null;
    }
}