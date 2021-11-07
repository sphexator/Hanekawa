// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Disqord;
// using Hanekawa.Infrastructure;
// using Hanekawa.Infrastructure.Entities;
// using Hanekawa.Infrastructure.Tables.Account;
// using Hanekawa.Infrastructure.Tables.Account.Achievement;
// using Microsoft.EntityFrameworkCore;
// TODO: Level Achievements
// namespace Hanekawa.Bot.Service.Achievements
// {
//     public partial class AchievementService
//     {
//         public async Task ServerLevel(IMember user, Account userData, DbService db)
//         {
//             var achievements = await db.Achievements
//                 .Where(x => x.Category == AchievementCategory.Level && x.Requirement <= userData.Level)
//                 .ToListAsync();
//
//             if (achievements == null || achievements.Count == 0) return;
//             var unlocks = await db.AchievementUnlocks.Where(x => x.UserId == userData.UserId).ToListAsync();
//             var toAdd = (from x in achievements
//                 where unlocks.All(e => e.AchieveId != x.AchievementId)
//                 select new AccountAchievement
//                 {
//                     Date = DateTimeOffset.UtcNow,
//                     Id = Guid.NewGuid(),
//                     UserId = new Snowflake(userData.UserId),
//                     Achievement = x,
//                     AchieveId = x.AchievementId
//                 }).ToList();
//
//             await db.AchievementUnlocks.AddRangeAsync(toAdd);
//             await db.SaveChangesAsync();
//         }
//     }
// }