using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Administration.Warning
{
    public partial class WarnService : INService, IJob
    {
        private readonly InternalLogService _log;
        private readonly LogService _logService;

        public WarnService(LogService logService, InternalLogService log)
        {
            _logService = logService;
            _log = log;
        }

        public Task Execute(IJobExecutionContext context) => VoidWarning();

        private async Task VoidWarning()
        {
            using (var db = new DbService())
            {
                await db.Warns.Where(x => x.Time.AddDays(7).Date <= DateTime.UtcNow.Date)
                    .ForEachAsync(x => x.Valid = false);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddWarn(DbService db, SocketGuildUser user, SocketGuildUser staff, string reason,
            WarnReason warnType,
            bool notify = false, TimeSpan? muteTime = null)
        {
            var number = await db.Warns.CountAsync(x => x.GuildId == user.Guild.Id);
            await db.Warns.AddAsync(new Warn
            {
                Id = number + 1,
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Moderator = staff.Id,
                MuteTimer = muteTime,
                Reason = reason,
                Time = DateTime.UtcNow,
                Type = warnType,
                Valid = true
            });
            await db.SaveChangesAsync();
            await NotifyUser(user, staff, warnType, reason, muteTime);
            await _logService.Warn(user, staff, reason, db);
        }

        private async Task NotifyUser(SocketGuildUser user, IMentionable staff, WarnReason type, string reason,
            TimeSpan? duration = null)
        {
            try
            {
                if (reason.IsNullOrWhiteSpace()) reason = "No reason provided";
                var dm = await user.GetOrCreateDMChannelAsync();
                var content = new StringBuilder();
                content.AppendLine($"You've been {type} in {user.Guild.Name} by {staff.Mention}");
                content.AppendLine($"Reason: {reason.ToLower()}");
                var embed = new EmbedBuilder().CreateDefault(content.ToString(), user.Guild.Id);
                if (duration != null) embed.AddField("Duration", $"{duration.Value.Humanize()} ({duration.Value})");
                await dm.SendMessageAsync(null, false, embed.Build());
            }
            catch
            {
                /* IGNORE, maybe I shouldn't ignore this ? handle what kind of exception is thrown, if user has dms closed, ignore else log it*/
            }
        }
    }
}