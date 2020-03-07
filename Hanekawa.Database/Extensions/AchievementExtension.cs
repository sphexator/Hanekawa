using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Achievement;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<AchievementTracker> GetOrCreateAchievementProgress(this DbService context, CachedMember user,
            int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, user.Id).ConfigureAwait(false);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = user.Id
            };
            try
            {
                await context.AchievementTrackers.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.AchievementTrackers.FindAsync(type, user.Id).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<AchievementTracker> GetOrCreateAchievementProgress(this DbService context, ulong userId,
            int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, userId).ConfigureAwait(false);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = userId
            };
            try
            {
                await context.AchievementTrackers.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.AchievementTrackers.FindAsync(type, userId).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }
    }
}