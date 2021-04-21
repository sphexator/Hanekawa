using System;
using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class LevelExpEvent
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelId { get; set; }
        public Snowflake? MessageId { get; set; }
        public double Multiplier { get; set; }
        public DateTime Time { get; set; }
    }
}