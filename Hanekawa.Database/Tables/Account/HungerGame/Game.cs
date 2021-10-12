using System;
using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class Game
    {
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        public int Round { get; set; }
        
        public List<Participants> Participants { get; set; }
    }
}