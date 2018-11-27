using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Hanekawa.Services.Administration
{
    public enum WarnReason
    {
        Warning,
        Mute
    }

    public class WarnService : IJob, IHanaService, IRequiredService
    {
        private readonly DbService _db;

        public WarnService(DbService db)
        {
            _db = db;
            Console.WriteLine("1: WarnService Db initiated");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await VoidWarning();
        }

        public event AsyncEvent<SocketGuildUser, string, string> UserWarned;
        public event AsyncEvent<SocketGuildUser, string, string, TimeSpan> UserMuted;

        private static async Task VoidWarning()
        {
            using (var db = new DbService())
            {
                await db.Warns.Where(x => x.Time.AddDays(7).Date <= DateTime.UtcNow.Date)
                    .ForEachAsync(x => x.Valid = false);
                await db.SaveChangesAsync();
            }
        }

        public async Task<EmbedBuilder> GetSimpleWarnlogAsync(SocketGuildUser user)
        {
            var userdata = await _db.GetOrCreateUserData(user);
            var roleList = (from x in user.Roles where x.Name != "@everyone" select x.Name).ToList();
            var roles = string.Join(", ", roleList);
            var content = "**⮞ User Information**\n" +
                          $"Status: {GetStatus(user)}\n" +
                          $"{GetGame(user)}\n" +
                          $"Created: {user.CreatedAt.Humanize()} ({user.CreatedAt})\n" +
                          "\n" +
                          "**⮞ Member Information**\n" +
                          $"Joined: {user.JoinedAt.Humanize()} ({user.JoinedAt})\n" +
                          $"Roles: {roles}\n" +
                          "\n" +
                          "**⮞ Activity**\n" +
                          $"Last Message: {userdata.LastMessage.Humanize()} \n" +
                          $"First Message: {userdata.FirstMessage.Humanize()} \n" +
                          "\n" +
                          "**⮞ Session**\n" +
                          $"Amount: {userdata.Sessions}\n" +
                          $"Time: {userdata.StatVoiceTime.Humanize()} ({userdata.StatVoiceTime})";
            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatar(),
                Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Id})"
            };
            var embed = new EmbedBuilder
            {
                Description = content,
                Author = author,
                Color = Color.Purple,
                Fields = await GetWarnings(user)
            };
            return embed;
        }

        public async Task<IEnumerable<string>> GetFullWarnlogAsync(SocketGuildUser user)
        {
            var result = new List<string>();
            var warnings = await _db.Warns.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                .ToListAsync();
            for (var i = 0; i < warnings.Count;)
            {
                string page = null;
                for (var j = 0; j < 5; j++)
                {
                    var index = warnings[i];
                    page = $"Warn ID: {index.Id}\n" +
                           $"{index.Type} - <@{index.Moderator}>\n" +
                           $"{index.Reason.Truncate(700) ?? "I made this :)"}\n" +
                           $"{index.Time.Humanize()}\n";
                    if (index.MuteTimer != null) page += $"{index.MuteTimer.Value.Humanize()}\n";
                    page += "\n";
                    i++;
                }

                result.Add(page);
            }

            return result;
        }

        public async Task AddWarning(SocketGuildUser user, IUser staff, DateTime time, string reason, WarnReason type,
            IEnumerable<IMessage> messages = null)
        {
            var number = await _db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
            var data = new Warn
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Moderator = staff.Id,
                Time = time,
                Reason = reason,
                Type = (Addons.Database.Data.WarnReason) type,
                Valid = true,
                Id = number + 1
            };
            await _db.Warns.AddAsync(data);
            await _db.SaveChangesAsync();
            if (messages != null)
            {
                var warn = await _db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }

            await WarnUser(WarnReason.Warning, user, staff, reason);
            await UserWarned(user, staff.Mention, reason);
        }

        public async Task AddWarning(SocketGuildUser user, IUser staff, DateTime time, string reason, WarnReason type,
            TimeSpan after, IEnumerable<IMessage> messages = null, bool notify = false)
        {
            var number = await _db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
            var data = new Warn
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Moderator = staff.Id,
                Time = time,
                Reason = reason,
                Type = (Addons.Database.Data.WarnReason) type,
                Valid = true,
                Id = number + 1,
                MuteTimer = after
            };
            await _db.Warns.AddAsync(data);
            await _db.SaveChangesAsync();

            if (messages != null)
            {
                var warn = await _db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }

            await WarnUser(WarnReason.Mute, user, staff, reason, after);
        }

        public async Task AddWarning(SocketGuildUser user, ulong staff, DateTime time, string reason, WarnReason type,
            IEnumerable<IMessage> messages = null, bool notify = false)
        {
            var number = await _db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
            var data = new Warn
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Moderator = staff,
                Time = time,
                Reason = reason,
                Type = (Addons.Database.Data.WarnReason) type,
                Valid = true,
                Id = number + 1,
                MuteTimer = null
            };
            await _db.Warns.AddAsync(data);
            await _db.SaveChangesAsync();
            if (messages != null)
            {
                var warn = await _db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }

            await WarnUser(WarnReason.Warning, user, "Auto-Moderator", reason);
            await UserWarned(user, "Auto-Moderator", reason);
        }

        public async Task AddWarning(SocketGuildUser user, ulong staff, DateTime time, string reason, WarnReason type,
            TimeSpan after, IEnumerable<IMessage> messages = null, bool notify = false)
        {
            var number = await _db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
            var data = new Warn
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Moderator = staff,
                Time = time,
                Reason = reason,
                Type = (Addons.Database.Data.WarnReason) type,
                Valid = true,
                Id = number + 1,
                MuteTimer = after
            };
            await _db.Warns.AddAsync(data);
            await _db.SaveChangesAsync();

            if (messages != null)
            {
                var warn = await _db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }

            await WarnUser(WarnReason.Mute, user, "Auto-Moderator", reason, after);
            await UserMuted(user, "Auto-Moderator", reason, after);
        }

        private static async Task WarnUser(WarnReason warn, IGuildUser user, IMentionable staff, string reason)
        {
            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"You've been warned on {user.Guild.Name} by {staff.Mention}\n" +
                                  "Reason:\n" +
                                  $"{reason}"
                };
                await dm.SendEmbedAsync(embed);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private static async Task WarnUser(WarnReason warn, IGuildUser user, IMentionable staff, string reason,
            TimeSpan after)
        {
            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"You've been muted on {user.Guild.Name} by {staff.Mention}\n" +
                                  "Reason:\n" +
                                  $"{reason}"
                };
                embed.AddField("Duration", $"{after.Humanize()} ({after})");
                await dm.SendEmbedAsync(embed);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private static async Task WarnUser(WarnReason warn, IGuildUser user, string staff, string reason)
        {
            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"You've been warned on {user.Guild.Name} by {staff}\n" +
                                  "Reason:\n" +
                                  $"{reason}"
                };
                await dm.SendEmbedAsync(embed);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private static async Task WarnUser(WarnReason warn, IGuildUser user, string staff, string reason,
            TimeSpan after)
        {
            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = $"You've been muted on {user.Guild.Name} by {staff}\n" +
                                  "Reason:\n" +
                                  $"{reason}"
                };
                embed.AddField("Duration", $"{after.Humanize()} ({after})");
                await dm.SendEmbedAsync(embed);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private async Task<List<EmbedFieldBuilder>> GetWarnings(IGuildUser user)
        {
            var warns = await _db.Warns.Where(x => x.GuildId == user.GuildId).Where(y => y.UserId == user.Id)
                .ToListAsync();
            var result = new List<EmbedFieldBuilder>();
            foreach (var x in warns)
            {
                if (!x.Valid) continue;
                var content = $"{x.Type} - <@{x.Moderator}>\n" +
                              $"{x.Reason ?? "I made this :)"}\n" +
                              $"{x.Time.Humanize()}\n";
                var field = new EmbedFieldBuilder
                {
                    Name = $"Warn ID: {x.Id}",
                    IsInline = true,
                    Value = content.Truncate(999)
                };
                result.Add(field);
            }

            return result;
        }

        private async Task StoreMessages(int id, IGuildUser user, IEnumerable<IMessage> messages)
        {
            var result = messages.Select(x => new WarnMsgLog
                {
                    WarnId = id,
                    UserId = user.Id,
                    MsgId = x.Id,
                    Author = x.Author.Username,
                    Message = x.Content,
                    Time = x.Timestamp.UtcDateTime
                })
                .ToList();
            await _db.WarnMsgLogs.AddRangeAsync(result);
        }

        private static string GetStatus(IPresence user)
        {
            var result = "N/A";
            switch (user.Status)
            {
                case UserStatus.Online:
                    result = "Online";
                    break;
                case UserStatus.Idle:
                    result = "Idle";
                    break;
                case UserStatus.AFK:
                    result = "AFK";
                    break;
                case UserStatus.DoNotDisturb:
                    result = "DND";
                    break;
                case UserStatus.Invisible:
                    result = "Invisible";
                    break;
                case UserStatus.Offline:
                    result = "Offline";
                    break;
            }

            return result;
        }

        private static string GetGame(IPresence user)
        {
            if (user.Activity == null) return "Currently not playing";
            var result = "Currently not playing";
            switch (user.Activity.Type)
            {
                case ActivityType.Listening:
                    result = $"Listening: {user.Activity.Name}";
                    break;
                case ActivityType.Playing:
                    result = $"Playing: {user.Activity.Name}";
                    break;
                case ActivityType.Streaming:
                    result = $"Streaming: {user.Activity.Name}";
                    break;
                case ActivityType.Watching:
                    result = $"Watching: {user.Activity.Name}";
                    break;
            }

            return result;
        }
    }
}