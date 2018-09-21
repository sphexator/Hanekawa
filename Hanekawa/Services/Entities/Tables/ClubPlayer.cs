using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class ClubPlayer
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public uint Rank { get; set; }
        public DateTimeOffset JoinDate { get; set; }
    }
}
