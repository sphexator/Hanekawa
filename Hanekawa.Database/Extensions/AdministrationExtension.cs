using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<ModLog> CreateCaseId(this DbService context, IUser user, CachedGuild guild,
            DateTime time, ModAction action)
        {
            var counter = await context.ModLogs.CountAsync(x => x.GuildId == guild.Id.RawValue).ConfigureAwait(false);
            var data = new ModLog
            {
                Id = counter + 1,
                GuildId = guild.Id.RawValue,
                UserId = user.Id.RawValue,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.ModLogs.FirstOrDefaultAsync(x =>
                x.Date == time && x.UserId == user.Id.RawValue && x.GuildId == guild.Id.RawValue).ConfigureAwait(false);
        }

        public static async Task<Report> CreateReport(this DbService context, IUser user, CachedGuild guild,
            DateTime time)
        {
            var counter = await context.Reports.CountAsync(x => x.GuildId == guild.Id.RawValue).ConfigureAwait(false);
            var nr = counter == 0 ? 1 : counter + 1;

            var data = new Report
            {
                Id = nr,
                GuildId = guild.Id.RawValue,
                UserId = user.Id.RawValue,
                Status = true,
                Date = time
            };
            await context.Reports.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.Reports.FirstOrDefaultAsync(x => x.Date == time).ConfigureAwait(false);
        }
    }
}
