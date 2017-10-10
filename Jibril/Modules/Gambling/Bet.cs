using Discord.Addons.Preconditions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Gambling
{
    public class Bet
    {
        public static TimeSpan span = new TimeSpan(0, 0, 05);
        [Command("bet")]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(12, 1, Measure.Minutes, false, false)]
        public async Task HardBet(uint bet)
        {

        }

    }
}
