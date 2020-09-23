using System;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameStatus
    {
        public ulong GuildId { get; set; }
        public ulong? SignUpChannel { get; set; } = null;
        public ulong? EventChannel { get; set; } = null;
        public string EmoteMessageFormat { get; set; } 

        public HungerGameStage Stage { get; set; } = HungerGameStage.Closed;
        public DateTimeOffset SignUpStart { get; set; }
        public ulong? SignUpMessage { get; set; }
        public Guid? GameId { get; set; } = null;
    }
}