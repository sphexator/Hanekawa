using System;

namespace Hanekawa.Addons.Database.Tables.Club
{
    public class ClubUser
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int Rank { get; set; } = 3;
        public DateTimeOffset JoinDate { get; set; } = DateTimeOffset.UtcNow;
    }
}