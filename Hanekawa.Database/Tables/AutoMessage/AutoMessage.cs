using System;

namespace Hanekawa.Database.Tables.AutoMessage
{
    public class AutoMessage
    {
        public ulong GuildId { get; set; }
        public ulong Creator { get; set; }
        public ulong ChannelId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public TimeSpan Interval { get; set; }
    }
}