using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Moderation;
using Hanekawa.Core;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Administration.Warning
{
    public partial class WarnService : INService, IJob
    {
        private readonly DbService _db;
        public WarnService(DbService db) => _db = db;

        public Task Execute(IJobExecutionContext context) => VoidWarning();
        private async Task VoidWarning()
        {
            await _db.Warns.Where(x => x.Time.AddDays(7).Date <= DateTime.UtcNow.Date)
                    .ForEachAsync(x => x.Valid = false);
            await _db.SaveChangesAsync();
        }

        public async Task AddWarn(SocketGuildUser user, SocketGuildUser staff, string reason, WarnReason warnType,
            bool notify = false, TimeSpan? muteTime = null)
        {
            var number = await _db.Warns.CountAsync(x => x.GuildId == user.Guild.Id);
            await _db.Warns.AddAsync(new Warn
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
            await _db.SaveChangesAsync();
            await NotifyUser(user, staff, warnType, reason, muteTime);
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
                if(duration != null) embed.AddField("Duration", $"{duration.Value.Humanize()} ({duration.Value})");
                await dm.SendMessageAsync(null, false, embed.Build());
            }
            catch
            {
                /* IGNORE */
            }
        }
    }
}