using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService
    {
        public async Task DropAchievement(Account userData, DbService db)
        {
            var achievements = await db.Achievements
                .Where(x => x.Category == AchievementCategory.Drop && x.Requirement <= userData.DropClaims)
                .ToListAsync();
            if (achievements == null || achievements.Count == 0) return;
            var globalUser = await db.GetOrCreateGlobalUserDataAsync(userData.UserId);
            var unlocks = await db.AchievementUnlocks.Where(x => x.UserId == userData.UserId).ToListAsync();
            var toAdd = (from x in achievements
                where unlocks.All(e => e.AchieveId != x.AchievementId)
                select new AchievementUnlocked
                {
                    Date = DateTimeOffset.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = new Snowflake(userData.UserId),
                    Account = globalUser,
                    Achievement = x,
                    AchieveId = x.AchievementId
                }).ToList();

            await db.AchievementUnlocks.AddRangeAsync(toAdd);
            await db.SaveChangesAsync();
        }
    }
}
