using Discord;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Services.Administration
{
    public enum WarnReason
    {
        Warning,
        Mute
    }

    public class WarnService : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await VoidWarning();
        }

        private static async Task VoidWarning()
        {
            using (var db = new DbService())
            {
                await db.Warns.Where(x => x.Time.AddDays(7) <= DateTime.UtcNow)
                    .ForEachAsync(x => x.Valid = false);
                await db.SaveChangesAsync();
            }
        }

        public async Task<EmbedBuilder> Warnlog(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var roleList = (from x in user.Roles where x.Name != "@everyone" select x.Name).ToList();
                var roles = string.Join(", ", roleList);
                var content = $"**>User Information**\n" +
                              $"Status: {user.Status}\n" +
                              $"Game: {user.Activity}\n" +
                              $"Created: {user.CreatedAt.Humanize()}({user.CreatedAt})\n" +
                              $"\n" +
                              $"**>Member Information**\n" +
                              $"Joined: {user.JoinedAt.Humanize()}({user.JoinedAt})\n" +
                              $"Roles: {roles}\n" +
                              $"\n" +
                              $"**>Activity**\n" +
                              $"Last Message: {userdata.LastMessage.Humanize()} \n" +
                              $"First Message: {userdata.LastMessage.Humanize()}";
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = $"{user.Username}#{user.DiscriminatorValue} ({user.Id})"
                };
                var embed = new EmbedBuilder
                {
                    Description = content,
                    Author = author,
                    Fields = await GetWarnings(user)
                };
                return embed;
            }
        }

        public async Task AddWarning(IGuildUser user, IUser staff, DateTime time, string reason, WarnReason type, IEnumerable<IMessage> messages = null)
        {
            using (var db = new DbService())
            {
                var data = new Warn
                {
                    GuildId = user.GuildId,
                    UserId = user.Id,
                    Moderator = staff.Id,
                    Time = time,
                    Reason = reason,
                    Type = type,
                    Valid = true
                };
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();
                if (messages == null) return;
                var warn = await db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }
        }

        public async Task AddWarning(IGuildUser user, ulong staff, DateTime time, string reason, WarnReason type, IEnumerable<IMessage> messages = null)
        {
            using (var db = new DbService())
            {
                var data = new Warn
                {
                    GuildId = user.GuildId,
                    UserId = user.Id,
                    Moderator = staff,
                    Time = time,
                    Reason = reason,
                    Type = type,
                    Valid = true
                };
                await db.Warns.AddAsync(data);
                await db.SaveChangesAsync();
                if (messages == null) return;
                var warn = await db.Warns.Where(x => x.Time == time).FirstOrDefaultAsync(x => x.UserId == user.Id);
                await StoreMessages(warn.Id, user, messages);
            }
        }

        private static async Task<List<EmbedFieldBuilder>> GetWarnings(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var warns = await db.Warns.Where(x => x.GuildId == user.GuildId).Where(y => y.UserId == user.Id).ToListAsync();
                var result = new List<EmbedFieldBuilder>();
                foreach (var x in warns)
                {
                    if (!x.Valid) continue;
                    var field = new EmbedFieldBuilder
                    {
                        Name = $"Warn ID: {x.Id}",
                        IsInline = false,
                        Value = $"<@{x.Moderator}>\n" +
                                $"{x.Reason}\n" +
                                $"{x.Reason}"
                    };
                    result.Add(field);
                }

                return result;
            }
        }

        private static async Task<List<EmbedFieldBuilder>> GetAllWarnings(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var result = new List<EmbedFieldBuilder>();
                foreach (var x in await db.Warns.Where(x => x.GuildId == user.GuildId).Where(y => y.UserId == user.Id).ToListAsync())
                {
                    var field = new EmbedFieldBuilder
                    {
                        Name = $"Warn ID: {x.Id}",
                        IsInline = false,
                        Value = $"<@{x.Moderator}>\n" +
                                $"{x.Reason}\n" +
                                $"{x.Reason}"
                    };
                    result.Add(field);
                }

                return result;
            }
        }

        private static async Task StoreMessages(int id, IGuildUser user, IEnumerable<IMessage> messages)
        {
            using (var db = new DbService())
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
                await db.WarnMsgLogs.AddRangeAsync(result);
            }
        }
    }
}
