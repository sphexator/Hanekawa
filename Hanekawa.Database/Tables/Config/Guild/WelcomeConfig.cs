﻿using System;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class WelcomeConfig
    {
        public ulong GuildId { get; set; }
        public ulong? Channel { get; set; } = null;
        public int Limit { get; set; } = 4;
        public bool Banner { get; set; } = false;
        public string Message { get; set; }
        public int? Reward { get; set; } = null;
        public TimeSpan? TimeToDelete { get; set; } = null;
        public bool AutoDelOnLeave { get; set; } = false;
        public DateTimeOffset? IgnoreNew { get; set; } = null;
    }
}