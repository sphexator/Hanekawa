﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Moderation;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
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
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
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
                var embed = new EmbedBuilder()
                    .CreateDefault(content, user.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                        {IconUrl = user.GetAvatar(), Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Id})"})
                    .WithFields(await GetWarnings(db, user));
                return embed;
            }
        }

        public async Task<List<string>> GetFullWarnlogAsync(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var result = new List<string>();
                foreach (var x in await db.Warns.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                    .ToListAsync())
                    result.Add($"Warn ID: {x.Id}\n" +
                               $"{x.Type} - <@{x.Moderator}>\n" +
                               $"{x.Reason.Truncate(700) ?? "I made this :)"}\n" +
                               $"{x.Time.Humanize()}\n");
                if (result.Count == 0) result.Add("User has no warnings");
                return result;
            }
        }

        public async Task AddWarning(SocketGuildUser user, IUser staff, DateTime time, string reason, WarnReason type,
            IEnumerable<IMessage> messages = null)
        {
            using (var db = new DbService())
            {
                var number = await db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
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
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();

                await WarnUser(WarnReason.Warning, user, staff, reason);
                await UserWarned(user, staff.Mention, reason);
            }
        }

        public async Task AddWarning(SocketGuildUser user, IUser staff, DateTime time, string reason, WarnReason type,
            TimeSpan after, IEnumerable<IMessage> messages = null, bool notify = false)
        {
            using (var db = new DbService())
            {
                var number = await db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
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
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();

                await WarnUser(WarnReason.Mute, user, staff, reason, after);
            }
        }

        public async Task AddWarning(SocketGuildUser user, ulong staff, DateTime time, string reason, WarnReason type,
            IEnumerable<IMessage> messages = null, bool notify = false)
        {
            using (var db = new DbService())
            {
                var number = await db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
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
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();

                await WarnUser(WarnReason.Warning, user, "Auto-Moderator", reason);
                await UserWarned(user, "Auto-Moderator", reason);
            }
        }

        public async Task AddWarning(SocketGuildUser user, ulong staff, DateTime time, string reason, WarnReason type,
            TimeSpan after, IEnumerable<IMessage> messages = null, bool notify = false)
        {
            using (var db = new DbService())
            {
                var number = await db.Warns.Where(x => x.GuildId == user.Guild.Id).CountAsync();
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
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();

                await WarnUser(WarnReason.Mute, user, "Auto-Moderator", reason, after);
                await UserMuted(user, "Auto-Moderator", reason, after);
            }
        }

        private static async Task WarnUser(WarnReason warn, IGuildUser user, IMentionable staff, string reason)
        {
            try
            {
                var dm = await user.GetOrCreateDMChannelAsync();
                var embed = new EmbedBuilder().CreateDefault(
                    $"You've been warned in {user.Guild.Name} by {staff.Mention}\n" +
                    "Reason:\n" +
                    $"{reason}", user.GuildId);
                await dm.ReplyAsync(embed);
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
                var embed = new EmbedBuilder().CreateDefault(
                    $"You've been muted on {user.Guild.Name} by {staff.Mention}\n" +
                    "Reason:\n" +
                    $"{reason}", user.GuildId);
                embed.AddField("Duration", $"{after.Humanize()} ({after})");
                await dm.ReplyAsync(embed);
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
                var embed = new EmbedBuilder().CreateDefault($"You've been warned in {user.Guild.Name} by {staff}\n" +
                                                             "Reason:\n" +
                                                             $"{reason}", user.GuildId);
                await dm.ReplyAsync(embed);
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
                var embed = new EmbedBuilder().CreateDefault($"You've been muted in {user.Guild.Name} by {staff}\n" +
                                                             "Reason:\n" +
                                                             $"{reason}", user.GuildId);
                embed.AddField("Duration", $"{after.Humanize()} ({after})");
                await dm.ReplyAsync(embed);
            }
            catch
            {
                /* IGNORE */
            }
        }

        private async Task<List<EmbedFieldBuilder>> GetWarnings(DbService db, IGuildUser user)
        {
            var result = new List<EmbedFieldBuilder>();
            foreach (var x in await db.Warns.Where(x => x.GuildId == user.GuildId).Where(y => y.UserId == user.Id)
                .ToListAsync())
            {
                if (!x.Valid) continue;
                var content = $"{x.Type} - <@{x.Moderator}>\n" +
                              $"{x.Reason ?? "No reason provided."}\n" +
                              $"{x.Time.Humanize()}\n";
                result.Add(new EmbedFieldBuilder
                    {Name = $"Warn ID: {x.Id}", IsInline = true, Value = content.Truncate(999)});
            }

            return result;
        }

        private static string GetStatus(IPresence user)
        {
            string result;
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
                default:
                    result = "N/A";
                    break;
            }

            return result;
        }

        private static string GetGame(IPresence user)
        {
            string result;
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
                default:
                    result = "Currently not playing";
                    break;
            }

            return result;
        }
    }
}