using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.AuditLogs;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        public async Task BanAsync(BanCreatedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if (guild == null) return;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogBan.HasValue) return;
                var channel = guild.GetChannel(cfg.LogBan.Value);
                if(channel == null) return;

                var caseId = await db.CreateCaseId(e.User, guild, DateTime.UtcNow, ModAction.Ban);
                IMember mod;
                var banCacheUser = _cache.TryGetBanCache(guild.Id, e.UserId);
                if (banCacheUser != null)
                {
                    mod = await _bot.GetOrFetchMemberAsync(e.GuildId, banCacheUser.Value) ??
                          await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberBanned);
                    if (mod != null) caseId.ModId = mod.Id.RawValue;
                }
                else mod = await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberBanned);

                var embed = new LocalEmbedBuilder
                {
                    Color = HanaBaseColor.Red(),
                    Author = new LocalEmbedAuthorBuilder { Name = $"User Banned | Case ID: {caseId.Id} | {e.User}" },
                    Fields =
                        {
                            new LocalEmbedFieldBuilder {Name = "User", Value = e.User.Mention, IsInline = false},
                            new LocalEmbedFieldBuilder {Name = "Moderator", Value = mod?.Mention ?? "N/A", IsInline = false},
                            new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false}
                        },
                    Footer = new LocalEmbedFooterBuilder { Text = $"User ID: {e.User.Id.RawValue}", IconUrl = e.User.GetAvatarUrl() },
                    Timestamp = DateTimeOffset.UtcNow
                };
                if (e.User is CachedMember {JoinedAt: {HasValue: true}} member)
                    embed.AddField("Time In Server", (DateTimeOffset.UtcNow - member.JoinedAt.Value).Humanize(5));

                var msg = await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    IsTextToSpeech = false,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    Attachments = null,
                    Content = null
                }.Build());
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"(Log Service) Error in {guild.Id.RawValue} for Ban Log - {exception.Message}");
            }
        }

        public async Task UnbanAsync(BanDeletedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if (guild == null) return;

            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogBan.HasValue) return;
                var channel = guild.GetChannel(cfg.LogBan.Value);
                if(channel == null) return;
                var caseId = await db.CreateCaseId(e.User, guild, DateTime.UtcNow, ModAction.Unban);
                
                IMember mod = null;
                var banCacheUser = _cache.TryGetBanCache(guild.Id, e.UserId);
                if (banCacheUser != null)
                {
                    mod = await _bot.GetOrFetchMemberAsync(e.GuildId, banCacheUser.Value) ??
                          await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberUnbanned);
                    if (mod != null) caseId.ModId = mod.Id;
                }
                else mod = await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberUnbanned);

                var embed = new LocalEmbedBuilder
                {
                    Color = HanaBaseColor.Lime(),
                    Author = new LocalEmbedAuthorBuilder { Name = $"User Unbanned | Case ID: {caseId.Id} | {e.User}" },
                    Footer = new LocalEmbedFooterBuilder { Text = $"User ID: {e.User.Id.RawValue}", IconUrl = e.User.GetAvatarUrl() },
                    Timestamp = DateTimeOffset.UtcNow,
                    Fields =
                    {
                        new LocalEmbedFieldBuilder {Name = "User", Value = $"{e.User.Mention}", IsInline = true},
                        new LocalEmbedFieldBuilder {Name = "Moderator", Value = mod?.Mention ?? "N/A", IsInline = true},
                        new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = true}
                    }
                };
                var msg = await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    IsTextToSpeech = false,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    Attachments = null,
                    Content = null
                }.Build());
                caseId.MessageId = msg.Id.RawValue;
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"(Log Service) Error in {e.GuildId.RawValue} for UnBan Log - {exception.Message}");
            }
        }

        private async ValueTask<IMember> CheckAuditLog(IGuild guild, Snowflake userId, ModLog caseId, AuditLogActionType type)
        {
            IMember mod = null;
            await Task.Delay(TimeSpan.FromSeconds(2));
            IReadOnlyList<IAuditLog> audits;
            switch (type)
            {
                case AuditLogActionType.MemberBanned:
                    audits = await guild.FetchAuditLogsAsync<IMemberBannedAuditLog>(100);
                    break;
                case AuditLogActionType.MemberUnbanned:
                    audits = await guild.FetchAuditLogsAsync<IMemberUnbannedAuditLog>(100);
                    break;
                default:
                    return null;
            }
            var audit = audits.FirstOrDefault(x => x.TargetId.HasValue && x.TargetId.Value == userId);
            if (audit?.ActorId != null)
            {
                var temp = await _bot.GetOrFetchMemberAsync(guild.Id, audit.ActorId.Value);
                if (!temp.IsBot)
                {
                    caseId.ModId = temp.Id;
                    return temp;
                }

                var reasonSplit = audit.Reason.Replace("(", " ")
                    .Replace(")", " ")
                    .Replace("-", " ")
                    .Split(" ");
                var modId = FetchId(reasonSplit);
                if (modId.HasValue)
                {
                    temp = await _bot.GetOrFetchMemberAsync(guild.Id, modId.Value);
                    if (temp is {IsBot: false} && Discord.Permissions
                        .CalculatePermissions(guild, temp, temp.GetRoles().Values).BanMembers) mod = temp;
                }
            }

            if (mod == null) return null;
            caseId.ModId = mod.Id;
            return mod;
        }

        private static Snowflake? FetchId(IEnumerable<string> value)
        {
            foreach (var x in value)
            {
                if (Snowflake.TryParse(x, out var result)) return result;
            }

            return null;
        }
    }
}