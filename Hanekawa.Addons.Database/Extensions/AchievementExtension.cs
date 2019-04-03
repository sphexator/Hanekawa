using Discord;
using Hanekawa.Addons.Database.Tables.Achievement;
using System.Threading.Tasks;

namespace Hanekawa.Addons.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<AchievementTracker> GetOrCreateAchievementProgress(this DbService context, IGuildUser user,
            int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, user.Id);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = user.Id
            };
            await context.AchievementTrackers.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AchievementTrackers.FindAsync(type, user.Id);
        }

        public static async Task<AchievementTracker> GetOrCreateAchievementProgress(this DbService context, ulong userId,
            int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, userId);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = userId
            };
            await context.AchievementTrackers.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AchievementTrackers.FindAsync(type, userId);
        }
    }
}