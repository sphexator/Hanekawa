using System;

namespace Hanekawa.Entities.BoardConfig
{
    public class Board
    {
        public Board() { }
        public Board(ulong guildId, ulong userId, ulong messageId)
        {
            GuildId = guildId;
            UserId = userId;
            MessageId = messageId;
        }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public int StarAmount { get; set; } = 1;
        public DateTimeOffset? Boarded { get; set; } = DateTimeOffset.UtcNow;
    }
}