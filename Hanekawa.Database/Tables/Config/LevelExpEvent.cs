﻿using System;

namespace Hanekawa.Database.Tables.Config
{
    public class LevelExpEvent
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? MessageId { get; set; }
        public double Multiplier { get; set; }
        public DateTime Time { get; set; }
    }
}