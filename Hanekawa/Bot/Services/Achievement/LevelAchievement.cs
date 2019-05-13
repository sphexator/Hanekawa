using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Achievement
{
    public partial class AchievementService
    {
        public async Task ServerLevel(SocketGuildUser user, Account userData, DbService db)
        {
            var achievements = await db.Achievements.Where(x => x.TypeId == Level && !x.Once && !x.Global).ToListAsync();
            if (achievements == null || achievements.Count == 0) return;
            var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
            if (achievements.Any(x => x.Requirement == userData.Level))
            {
                var achieve = achievements.First(x => x.Requirement == userData.Level);
                var check = unlocked.FirstOrDefault(x => x.Achievement == achieve);
                if (check != null) return;
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = Level,
                    UserId = user.Id,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();
            }
            else
            {
                var belowAchieves = achievements
                    .Where(x => x.Requirement < userData.Level).ToList();
                if (belowAchieves.Count > 0)
                {
                    foreach (var x in belowAchieves)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Level,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task GlobalLevel(SocketGuildUser user, AccountGlobal userData, DbService db)
        {
            var achievements = await db.Achievements
                .Where(x => x.TypeId == Level && !x.Once && x.Global).ToListAsync();
            if (achievements == null || achievements.Count == 0) return;

            if (achievements.Any(x => x.Requirement == userData.Level))
            {
                var achieve = achievements.First(x => x.Requirement == userData.Level);
                var data = new AchievementUnlock
                {
                    AchievementId = achieve.AchievementId,
                    TypeId = Level,
                    UserId = user.Id,
                    Achievement = achieve
                };
                await db.AchievementUnlocks.AddAsync(data);
                await db.SaveChangesAsync();
            }
            else
            {
                var belowAchieves = achievements.Where(x => x.Requirement < userData.Level).ToList();
                if (belowAchieves.Count > 0)
                {
                    var unlocked = await db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                    foreach (var x in belowAchieves)
                    {
                        if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Level,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
