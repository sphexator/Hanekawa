using System;

namespace Hanekawa.HungerGames.Entity
{
    public class GameHistory
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong? Winner { get; set; } = null;
        public bool Active { get; set; } = true;
        public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? End { get; set; } = null;
    }
}