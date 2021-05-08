using System;
using Disqord;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGame
    {
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        public int Round { get; set; }
        public int Alive { get; set; }
        public int Participants { get; set; }
    }
}