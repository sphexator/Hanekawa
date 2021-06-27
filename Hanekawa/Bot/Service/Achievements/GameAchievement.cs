using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService
    {
        public async Task GameKill(Snowflake guildId, Snowflake userId, bool pvp)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(guildId, userId);
            if (pvp) userData.GamePvPAmount++;
            else userData.GameKillAmount++;
            var achievements = new List<Achievement>();
            if (pvp)
                achievements = await db.Achievements
                    .Where(x => x.Category == AchievementCategory.PvP && x.Requirement <= userData.GamePvPAmount)
                    .ToListAsync();
            else
                await db.Achievements
                    .Where(x => x.Category == AchievementCategory.Game && x.Requirement <= userData.GameKillAmount)
                    .ToListAsync();
            if (achievements == null || achievements.Count == 0)
            {
                await db.SaveChangesAsync();
                return;
            }

            var unlocks = await db.AchievementUnlocks.Where(x => x.UserId == userData.UserId).ToListAsync();
            var toAdd = (from x in achievements
                where unlocks.All(e => e.AchieveId != x.AchievementId)
                select new AchievementUnlocked
                {
                    Date = DateTimeOffset.UtcNow,
                    Id = Guid.NewGuid(),
                    UserId = new Snowflake(userData.UserId),
                    Achievement = x,
                    AchieveId = x.AchievementId
                }).ToList();

            if (toAdd.Count > 0) await db.AchievementUnlocks.AddRangeAsync(toAdd);
            await db.SaveChangesAsync();
        }
    }
}