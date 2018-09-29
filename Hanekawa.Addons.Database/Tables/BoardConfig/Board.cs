using System;

namespace Hanekawa.Addons.Database.Tables.BoardConfig
{
    public class Board
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public uint StarAmount { get; set; }
        public DateTimeOffset? Boarded { get; set; }
    }
}