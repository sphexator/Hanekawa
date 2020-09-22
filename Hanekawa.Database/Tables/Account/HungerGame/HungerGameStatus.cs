using System;
using System.Collections.Generic;
using System.Text;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameStatus
    {
        public ulong GuildId { get; set; }
        public HungerGameStage Stage { get; set; }
    }
}
