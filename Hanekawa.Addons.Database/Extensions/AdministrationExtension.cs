using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Addons.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<ModLog> CreateCaseId(this DbService context, IUser user, SocketGuild guild,
            DateTime time, ModAction action)
        {
            var counter = await context.ModLogs.CountAsync(x => x.GuildId == guild.Id);
            var data = new ModLog
            {
                Id = counter + 1,
                GuildId = guild.Id,
                UserId = user.Id,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ModLogs.FirstOrDefaultAsync(x =>
                x.Date == time && x.UserId == user.Id && x.GuildId == guild.Id);
        }

        public static async Task<Report> CreateReport(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.Reports.CountAsync(x => x.GuildId == guild.Id);
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
            await context.Reports.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Reports.FirstOrDefaultAsync(x => x.Date == time);
        }
    }
}
