using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService
    {
        public async Task ServerLevel(CachedMember user, Account userData)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var achievements = await db.Achievements
                .Where(x => x.Category == AchievementCategory.Level && x.Requirement <= userData.Level)
                .ToListAsync();
            
            if (achievements == null || achievements.Count == 0) return;
            var globalUser = await db.GetOrCreateGlobalUserDataAsync(user);
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
