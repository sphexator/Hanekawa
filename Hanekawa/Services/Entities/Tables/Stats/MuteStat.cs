﻿namespace Hanekawa.Services.Entities.Tables.Stats
{
    public class MuteStat
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int Amount { get; set; }
    }
}