using System;
using Disqord;

namespace Hanekawa.Database.Tables.BoardConfig
{
    public class Board
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public Snowflake MessageId { get; set; }
        public int StarAmount { get; set; } = 1;
        public DateTimeOffset? Boarded { get; set; } = DateTimeOffset.UtcNow;
    }
}