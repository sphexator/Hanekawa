﻿using System.Linq;
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
        public async Task DropClaim(CachedMember user, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
            var progress = await db.GetOrCreateAchievementProgress(user, Drop);

            if (progress == null) return;
            if (achievements == null) return;
            var progCount = progress.Count + 1;
            if (achievements.Any(x => x.Requirement == progCount && !x.Once))
            {
                var achieve = achievements.FirstOrDefault(x => x.Requirement == progCount && !x.Once);
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

                _log.LogAction(LogLevel.Information, $"(Achievement Service) {user.Id.RawValue} scored {achieve.Name} in {user.Guild.Id.RawValue}");
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount && !x.Once).ToList();
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