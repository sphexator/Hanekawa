using System;

namespace Hanekawa.Addons.Database.Tables.GuildConfig
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