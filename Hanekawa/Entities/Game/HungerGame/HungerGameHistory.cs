using System;
using System.Threading.Tasks;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Game.HungerGame
{
    public class HungerGameHistory
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; } = DateTimeOffset.UtcNow;
        public ulong Winner { get; set; }
        public ulong[] Participants { get; set; }

        public int Experience { get; set; } = 0;
        public int Credit { get; set; } = 0;
        public int SpecialCredit { get; set; } = 0;
        public ulong? Role { get; set; } = null;

        public async ValueTask AwardAsync(IGuildUser user, Account.Account userData)
        {
            
        }
    }
}