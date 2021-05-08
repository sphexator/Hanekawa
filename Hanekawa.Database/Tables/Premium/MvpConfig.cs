﻿using System;
using Disqord;

namespace Hanekawa.Database.Tables.Premium
{
    public class MvpConfig
    {
        public Snowflake GuildId { get; set; }
        public bool Disabled { get; set; }
        public DayOfWeek Day { get; set; } = DayOfWeek.Wednesday;
        public Snowflake? RoleId { get; set; } = null;
        public int Count { get; set; } = 5;
    }
}