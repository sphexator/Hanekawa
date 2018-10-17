using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Preconditions;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Achievement
{
    public class Achievement : InteractiveBase
    {
        [Command("achievement", RunMode = RunMode.Async)]
        [RequiredChannel]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task AchievementLog([Remainder] string tab = null)
        {
            var user = Context.User as IGuildUser;

        }
    }
}
