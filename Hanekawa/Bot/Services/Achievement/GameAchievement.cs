using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task PvpKill(CachedMember user, DbService db) => await PvpKill(user.Id.RawValue, user.Guild.Id.RawValue, db).ConfigureAwait(false);
        public async Task PvpKill(ulong userId, ulong guildId, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == PvP && !x.Once).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(userId, PvP);
            if (progress == null) return;

            if (achievements == null) return;

            if (achievements.Any(x => x.Requirement == progress.Count + 1 && !x.Once))
            {
                var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = PvP,
                    UserId = userId,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();

                _log.LogAction(LogLevel.Information, $"(Achievement Service) {userId} scored {achieve.Name} in {guildId}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == userId).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvP,
                            UserId = userId,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }

                    await db.SaveChangesAsync();
                }
            }

            progress.Count = progress.Count + 1;
            await db.SaveChangesAsync();
        }

        public async Task PveKill(CachedMember user, DbService db)
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

                _log.LogAction(LogLevel.Information, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.Guild.Id.RawValue}");
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