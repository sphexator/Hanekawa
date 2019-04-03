using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task DropClaim(SocketGuildUser user)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
            var progress = await _db.GetAchievementProgress(user, Drop);
        }
    }
}
