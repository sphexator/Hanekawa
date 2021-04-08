﻿using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Tables.Account.Achievement;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<AchievementTracker> GetOrCreateAchievementProgress(this DbService context, IMember user,
            int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, user.Id.RawValue).ConfigureAwait(false);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = user.Id.RawValue
            };
            try
            {
                await context.AchievementTrackers.AddAsync(data).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.AchievementTrackers.FindAsync(type, user.Id.RawValue).ConfigureAwait(false);
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