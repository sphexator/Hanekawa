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
        public async Task PvpKill(IMember user, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == PvP && !x.Once).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(user.Id.RawValue, PvP);
            if (progress == null) return;
            if (achievements == null) return;

            if (achievements.Any(x => x.Requirement == progress.Count + 1 && !x.Once))
            {
                var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = PvP,
                    UserId = user.Id.RawValue,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _logger.Log(LogLevel.Info, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.GuildId.RawValue}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == user.Id.RawValue).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvP,
                            UserId = user.Id.RawValue,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }

                    await db.SaveChangesAsync();
                }
            }

            progress.Count += 1;
            await db.SaveChangesAsync();
        }

        public async Task PveKill(IMember user, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == PvE && !x.Once).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(user.Id.RawValue, PvE);
            if (progress == null) return;
            if (achievements == null) return;

            var progCount = progress.Count + 1;

            if (achievements.Any(x => x.Requirement == progCount))
            {
                var achieve = achievements.First(x => x.Requirement == progCount);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = PvE,
                    UserId = user.Id.RawValue,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _logger.Log(LogLevel.Info, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.Id.RawValue}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == user.Id.RawValue).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvE,
                            UserId = user.Id.RawValue,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }

                    await db.SaveChangesAsync();
                }
            }

            progress.Count = progCount;
            await db.SaveChangesAsync();
        }
    }
}
