using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task TotalTime(SocketGuildUser user, TimeSpan time)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == Voice && x.AchievementNameId == 15).ToListAsync();
            var progress = await _db.GetOrCreateAchievementProgress(user, Voice);
            if (achievements == null || achievements.Count == 0) return;
            if (progress == null) return;

            var totalTime = Convert.ToInt32(time.TotalMinutes);
            var progCount = progress.Count + totalTime;
            if (achievements.Any(x => x.Requirement == progCount && !x.Once))
            {
                var achieve = achievements.FirstOrDefault(x =>
                    x.Requirement == progCount && x.Once == false);
                if (achieve != null)
                {
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Voice,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount && !x.Once).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await _db.AchievementUnlocks.Where(x => x.TypeId == Voice).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Voice,
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

        public async Task TimeAtOnce(SocketGuildUser user, TimeSpan time)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == Voice).ToListAsync();
            if (achievements == null || achievements.Count == 0) return;

            var totalTime = Convert.ToInt32(time.TotalMinutes);
            if (achievements.Any(x => x.Requirement == totalTime && x.AchievementNameId == 8))
            {
                var achieve = achievements.First(x => x.Requirement == totalTime && x.Once);
                var unlockCheck = await _db.AchievementUnlocks.FirstOrDefaultAsync(x =>
                    x.AchievementId == achieve.AchievementId && x.UserId == user.Id);
                if (unlockCheck != null) return;

                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = Voice,
                    UserId = user.Id,
                    Achievement = achieve
                };
                await _db.AchievementUnlocks.AddAsync(data);
                await _db.SaveChangesAsync();
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < totalTime).ToList();
                var unlock = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id && x.TypeId == Voice)
                    .ToListAsync();
                foreach (var x in below)
                {
                    if (unlock.Any(y => y.AchievementId == x.AchievementId)) continue;

                    var data = new AchievementUnlock
                    {
                        AchievementId = x.AchievementId,
                        TypeId = Level,
                        UserId = user.Id,
                        Achievement = x
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                }

                await _db.SaveChangesAsync();
            }
        }
    }
}
