using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task PvpKill(SocketGuildUser user)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == PvP && !x.Once).ToListAsync();
            var progress = await _db.GetOrCreateAchievementProgress(user.Id, PvP);
            if (progress == null) return;

            if (achievements == null) return;

            if (achievements.Any(x => x.Requirement == progress.Count + 1 && !x.Once))
            {
                var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = PvP,
                    UserId = user.Id,
                    Achievement = achieve
                };
                await _db.AchievementUnlocks.AddAsync(data);
                await _db.SaveChangesAsync();
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvP,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await _db.AchievementUnlocks.AddAsync(data);
                    }

                    await _db.SaveChangesAsync();
                }
            }

            progress.Count = progress.Count + 1;
            await _db.SaveChangesAsync();
        }

        public async Task PveKill(SocketGuildUser user)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == PvE && !x.Once).ToListAsync();
            var progress = await _db.GetOrCreateAchievementProgress(user.Id, PvE);
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
                    UserId = user.Id,
                    Achievement = achieve
                };
                await _db.AchievementUnlocks.AddAsync(data);
                await _db.SaveChangesAsync();
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = PvE,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await _db.AchievementUnlocks.AddAsync(data);
                    }

                    await _db.SaveChangesAsync();
                }
            }

            progress.Count = progCount;
            await _db.SaveChangesAsync();
        }
    }
}
