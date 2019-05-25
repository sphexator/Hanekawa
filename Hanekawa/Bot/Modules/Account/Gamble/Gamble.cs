using System.Threading.Tasks;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Gamble
{
    public class Gamble : InteractiveBase
    {
        [Name("Bet")]
        [Command("bet")]
        [Description("Gamble a certain amount and win up to 3x")]
        [Remarks("bet 50")]
        [RequiredChannel]
        public async Task BetAsync(int amount)
        {

        }
    }
}
