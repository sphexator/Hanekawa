using System;
using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Quote
{
    public class Quote
    {
        public ulong GuildId { get; set; }
        public string Key { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Added { get; set; } = DateTimeOffset.UtcNow;
        public ulong Creator { get; set; }
        public List<string> Triggers { get; set; } = new List<string>();
        public int LevelCap { get; set; } = 0;
    }
}