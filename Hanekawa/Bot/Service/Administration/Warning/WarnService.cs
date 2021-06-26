using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz;
using Quartz.Util;

namespace Hanekawa.Bot.Service.Administration.Warning
{
    public class WarnService : INService, IJob
    {
        private readonly Logger _logger;
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;

        public WarnService(Hanekawa bot, CacheService cache)
        {
            _bot = bot;
            _cache = cache;
            _logger = LogManager.GetCurrentClassLogger();
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public async Task Warn(IMember user, IMember staff, string reason, WarnReason type, bool notify, DbService db,
            TimeSpan? muteTime = null)
        {
            var number = await db.Warns.CountAsync(x => x.GuildId == user.GuildId);
            await db.Warns.AddAsync(new Warn
            {
                Id = number + 1,
                GuildId = user.GuildId,
                UserId = user.Id,
                Moderator = staff.Id,
                Reason = reason,
                Time = DateTime.UtcNow,
                Type = type,
                Valid = true
            });
            await db.SaveChangesAsync();
            if(notify) await NotifyUserAsync(user, staff, muteTime, reason, type);
            _logger.Log(LogLevel.Info, $"Warned {user.Id} in {user.GuildId}");
        }

        public async Task<List<LocalEmbed>> GetWarnLogAsync(IMember user, WarnLogType type, DbService db)
        {
            var userData = await db.GetOrCreateUserData(user);
            var toReturn = new List<LocalEmbed>();
            var sb = new StringBuilder();
            var warn = new StringBuilder();
            sb.AppendLine("**⮞ User Information**");
            sb.AppendLine($"Created: {user.CreatedAt().Humanize(DateTimeOffset.UtcNow)}");
            
            sb.AppendLine("**⮞ Member Information**");
            sb.AppendLine(user.JoinedAt.HasValue
                ? $"Joined: {user.JoinedAt.Value.Humanize(DateTimeOffset.UtcNow)}"
                : $"Joined: Not Available");

            sb.AppendLine("**⮞ Activity**");
            sb.AppendLine($"Last Message: {userData.LastMessage.Humanize(true, DateTime.UtcNow)}");
            sb.AppendLine($"First Message: {userData.FirstMessage.Humanize(true, DateTime.UtcNow)}");
            
            sb.AppendLine("**⮞ Session**");
            sb.AppendLine($"Amount: {userData.Sessions}"); 
            sb.AppendLine($"Time: {userData.StatVoiceTime.Humanize(2)} ({userData.StatVoiceTime})");
            sb.AppendLine("**⮞ Warnings**");
            foreach (var x in await GetWarnsAsync(user, type, db))
            {
                if (sb.Length + warn.Length + x.Length > 2000 && type != WarnLogType.Full)
                {
                    sb.AppendLine(warn.ToString());
                    toReturn.Add(new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor
                        {
                            Name = $"{user.Name}#{user.Discriminator} ({user.Id})",
                            IconUrl = user.GetAvatarUrl()
                        },
                        Color = _cache.GetColor(user.GuildId),
                        Description = sb.ToString()
                    });
                    if(type == WarnLogType.Simple) return toReturn;
                }

                warn.AppendLine(x);
                warn.AppendLine();
            }
            return toReturn;
        }
        
        private async Task<List<string>> GetWarnsAsync(IMember user, WarnLogType type, DbService db)
        {
            var warnings = type == WarnLogType.Full
                ? await db.Warns.Where(x => x.GuildId == user.GuildId && x.UserId == user.Id)
                    .OrderByDescending(x => x.Time).ToListAsync()
                : await db.Warns.Where(x =>
                        x.GuildId == user.GuildId && x.UserId == user.Id && x.Valid)
                    .OrderByDescending(x => x.Time).ToListAsync();
            var count = type == WarnLogType.Full && warnings.Count > 5 
                ? warnings.Count 
                : 5;
            if (count > warnings.Count) count = warnings.Count;
            var toReturn = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var x = warnings[i];
                var sb = new StringBuilder();
                sb.AppendLine($"Type: {x.Type}");
                sb.AppendLine($"Moderator: {await _bot.GetOrFetchMemberAsync(user.GuildId, x.Moderator)} ({x.Moderator})");
                sb.AppendLine($"Reason: {x.Reason}");
                sb.AppendLine($"Time: {x.Time.Humanize()}");
                toReturn.Add(sb.ToString());
            }
            
            return toReturn;
        }

        private async Task NotifyUserAsync(IMember user, IUser staff, TimeSpan? duration, string reason, WarnReason type)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(type == WarnReason.Muted
                    ? $"You've been muted in {_bot.GetGuild(user.GuildId).Name}"
                    : $"You've been warned in {_bot.GetGuild(user.GuildId).Name}");

                if (!reason.IsNullOrWhiteSpace()) sb.AppendLine($"Reason: {reason}");
                if (duration.HasValue) sb.AppendLine($"Duration: {duration.Value}");
                
                sb.AppendLine($"By: {staff.Name}#{staff.Discriminator} ({staff.Id})");
                
                await user.SendMessageAsync(new LocalMessage
                {
                    Content = sb.ToString(),
                    Attachments = null,
                    Embed = null,
                    AllowedMentions = LocalAllowedMentions.None,
                    Reference = null,
                    IsTextToSpeech = false
                });
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warn, e, "Couldn't DM user. DMs possibly closed.");
            }
        }
    }
}