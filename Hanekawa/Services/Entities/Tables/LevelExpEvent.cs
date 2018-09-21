using System;
using System.Collections.Generic;
using System.Text;

namespace Hanekawa.Services.Entities.Tables
{
    public class LevelExpEvent
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? MessageId { get; set; }
        public uint Multiplier { get; set; }
        public DateTime Time { get; set; }
    }
}
