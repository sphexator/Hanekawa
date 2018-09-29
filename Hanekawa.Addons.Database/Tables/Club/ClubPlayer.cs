using System;

namespace Hanekawa.Addons.Database.Tables.Club
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