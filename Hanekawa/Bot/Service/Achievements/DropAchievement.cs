using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService
    {
        public async Task DropAchievement(IMember user, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(user, Drop);

            if (progress == null) return;
            if (achievements == null) return;
            var progressCount = progress.Count + 1;
            if (achievements.Any(x => x.Requirement == progressCount && !x.Once))
            {
                var achieve = achievements.FirstOrDefault(x => x.Requirement == progressCount && !x.Once);
                if (achieve == null) return;
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = Drop,
                    Achievement = achieve,
                    UserId = user.Id.RawValue
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _logger.Log(LogLevel.Info, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.GuildId.RawValue}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progressCount && !x.Once).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == user.Id.RawValue).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;
                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Drop,
                            UserId = user.Id.RawValue,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }

                    await db.SaveChangesAsync();
                }
            }

            progress.Count++;
            await db.SaveChangesAsync();
        }
    }
}
