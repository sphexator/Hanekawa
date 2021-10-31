using System;
using Disqord;

namespace Hanekawa.Database.Tables.BoardConfig
{
    public class Board
    {
        public Board() { }
        public Board(Snowflake guildId, Snowflake userId, Snowflake messageId)
        {
            GuildId = guildId;
            UserId = userId;
            MessageId = messageId;
        }

        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public Snowflake MessageId { get; set; }
        public int StarAmount { get; set; } = 1;
        public DateTimeOffset? Boarded { get; set; } = DateTimeOffset.UtcNow;
    }
}