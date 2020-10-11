using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Shared;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Giveaway
{
    public class GiveawayService : INService
    {
        public async Task<bool> ActiveGiveaway(CachedMember user, DbService db, GiveawayType type)
        {

            return false;
        }
    }
}