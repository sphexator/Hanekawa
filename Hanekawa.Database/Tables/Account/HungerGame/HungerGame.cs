using System;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGame
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public int Round { get; set; }
        public int Alive { get; set; }
        public int Participants { get; set; }
    }
}