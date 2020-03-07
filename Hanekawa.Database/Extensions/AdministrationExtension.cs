using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<ModLog> CreateCaseId(this DbService context, CachedUser user, CachedGuild guild,
            DateTime time, ModAction action)
        {
            var counter = await context.ModLogs.CountAsync(x => x.GuildId == guild.Id).ConfigureAwait(false);
            var data = new ModLog
            {
                Id = counter + 1,
                GuildId = guild.Id,
                UserId = user.Id,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.ModLogs.FirstOrDefaultAsync(x =>
                x.Date == time && x.UserId == user.Id && x.GuildId == guild.Id).ConfigureAwait(false);
        }

        public static async Task<Report> CreateReport(this DbService context, CachedUser user, CachedGuild guild,
            DateTime time)
        {
            var counter = await context.Reports.CountAsync(x => x.GuildId == guild.Id).ConfigureAwait(false);
            int nr;
            if (counter == 0)
                nr = 1;
            else
                nr = counter + 1;

            var data = new Report
            {
                Id = nr,
                GuildId = guild.Id,
                UserId = user.Id,
                Status = true,
                Date = time
            };
            await context.Reports.AddAsync(data).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return await context.Reports.FirstOrDefaultAsync(x => x.Date == time).ConfigureAwait(false);
        }
    }
}
