using System;

namespace Hanekawa.Addons.Database.Tables.BoardConfig
{
    public class Board
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public int StarAmount { get; set; } = 1;
        public DateTimeOffset? Boarded { get; set; } = DateTimeOffset.UtcNow;
    }
}