﻿using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task DropClaim(SocketGuildUser user)
        {
            var achievements = await _db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
            var progress = await _db.GetOrCreateAchievementProgress(user, Drop);

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
                    UserId = user.Id
                };
                await _db.AchievementUnlocks.AddAsync(data);
                await _db.SaveChangesAsync();
            }
            else
            {
                var below = achievements.Where(x => x.Requirement < progCount && !x.Once).ToList();
                if (below.Count != 0)
                {
                    var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;
                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Drop,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await _db.AchievementUnlocks.AddAsync(data);
                    }

                    await _db.SaveChangesAsync();
                }
            }

            progress.Count++;
            await _db.SaveChangesAsync();
        }
    }
}
