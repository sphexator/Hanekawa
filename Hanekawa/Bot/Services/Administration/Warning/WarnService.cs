using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Administration.Warning
{
    public partial class WarnService : INService, IJob
    {
        private readonly InternalLogService _log;
        private readonly LogService _logService;
        private readonly ColourService _colourService;

        public WarnService(LogService logService, InternalLogService log, ColourService colourService)
        {
            _logService = logService;
            _log = log;
            _colourService = colourService;
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

        public async Task AddWarn(DbService db, CachedMember user, CachedMember staff, string reason,
            WarnReason warnType,
            bool notify = false, TimeSpan? muteTime = null)
        {
            var number = await db.Warns.CountAsync(x => x.GuildId == user.Guild.Id.RawValue);
            await db.Warns.AddAsync(new Warn
            {
                Id = number + 1,
                GuildId = user.Guild.Id.RawValue,
                UserId = user.Id.RawValue,
                Moderator = staff.Id.RawValue,
                MuteTimer = muteTime,
                Reason = reason,
                Time = DateTime.UtcNow,
                Type = warnType,
                Valid = true
            });
            await db.SaveChangesAsync();
            await NotifyUser(user, staff, warnType, reason, muteTime);
            await _logService.Warn(user, staff, reason, db);
            _log.LogAction(LogLevel.Information, $"(Warn Service) Warned {user.Id.RawValue} in {user.Guild.Id.RawValue}");
        }

        private async Task NotifyUser(CachedMember user, IMentionable staff, WarnReason type, string reason,
            TimeSpan? duration = null)
        {
            try
            {
                if (reason.IsNullOrWhiteSpace()) reason = "No reason provided";
                if (!(user.DmChannel is IDmChannel dm)) dm = await user.CreateDmChannelAsync();
                var content = new StringBuilder();
                content.AppendLine($"You've been {type} in {user.Guild.Name} by {staff.Mention}");
                content.AppendLine($"Reason: {reason.ToLower()}");
                var embed = new LocalEmbedBuilder().Create(content.ToString(), _colourService.Get(user.Guild.Id.RawValue));
                if (duration != null) embed.AddField("Duration", $"{duration.Value.Humanize(2)} ({duration.Value})");
                await dm.SendMessageAsync(null, false, embed.Build());
            }
            catch(Exception e)
            {
                _log.LogAction(LogLevel.Warning, e, $"(Warn Service) Couldn't direct message {user.Id.RawValue}, privacy settings?");
                /* IGNORE, maybe I shouldn't ignore this ? handle what kind of exception is thrown, if user has dms closed, ignore else log it*/
            }
        }
    }
}