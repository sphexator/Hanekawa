// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Disqord;
// using Hanekawa.Infrastructure;
// using Hanekawa.Infrastructure.Entities;
// using Hanekawa.Infrastructure.Tables.Account;
// using Hanekawa.Infrastructure.Tables.Account.Achievement;
// using Microsoft.EntityFrameworkCore;
// TODO Drop Achievements
// namespace Hanekawa.Bot.Service.Achievements
// {
//     public partial class AchievementService
//     {
//         public async Task DropAchievement(Account userData, DbService db)
//         {
//             var achievements = await db.Achievements
//                 .Where(x => x.Category == AchievementCategory.Drop && x.Requirement <= userData.DropClaims)
//                 .ToListAsync();
//             if (achievements == null || achievements.Count == 0) return;
//             var unlocks = await db.AchievementUnlocks.Where(x => x.UserId == userData.UserId).ToListAsync();
//             var toAdd = (from x in achievements
//                 where unlocks.All(e => e.AchievementId != x.AchievementId)
//                 select new AccountAchievement
//                 {
//                     DateAchieved = DateTimeOffset.UtcNow,
//                     AchievementId = Guid.NewGuid(),
//                     UserId = new Snowflake(userData.UserId),
//                     Achievement = x,
//                     User = x
//                 }).ToList();
//
//             await db.AchievementUnlocks.AddRangeAsync(toAdd);
//             await db.SaveChangesAsync();
//         }
//     }
// }