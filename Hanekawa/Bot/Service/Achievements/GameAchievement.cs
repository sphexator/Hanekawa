using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService
    {
        public async Task PvpKill(Snowflake userId)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var achievements = await db.Achievements.Where(x => x.TypeId == PvP && !x.Once).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(userId.RawValue, PvP);
            if (progress == null) return;
            if (achievements == null) return;

            if (achievements.Any(x => x.Requirement == progress.Count + 1 && !x.Once))
            {
                var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = PvP,
                    UserId = userId.RawValue,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _logger.Log(LogLevel.Info, $"(Achievement Service) {userId.RawValue} scored {achieve.Name}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == userId.RawValue).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvP,
                            UserId = userId.RawValue,
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

        public async Task PveKill(Snowflake userId)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var achievements = await db.Achievements.Where(x => x.TypeId == PvE && !x.Once).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(userId.RawValue, PvE);
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
                    UserId = userId.RawValue,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _logger.Log(LogLevel.Info, $"(Achievement Service) {userId.RawValue} scored {achieve.Name} in {userId.RawValue}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == userId.RawValue).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvE,
                            UserId = userId.RawValue,
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
