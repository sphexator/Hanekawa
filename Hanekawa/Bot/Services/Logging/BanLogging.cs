using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Disqord.Rest.AuditLogs;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Shared;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUnbanned(MemberUnbannedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guild = e.Guild;
                var user = e.User;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                    if (!cfg.LogBan.HasValue) return;
                    var channel = guild.GetTextChannel(cfg.LogBan.Value);
                    if (channel == null) return;
                    var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Unban);
                    var embed = new LocalEmbedBuilder
                    {
                        Color = Color.Green,
                        Author = new LocalEmbedAuthorBuilder {Name = $"User Unbanned | Case ID: {caseId.Id} | {user}"},
                        Footer = new LocalEmbedFooterBuilder {Text = $"User ID: {user.Id.RawValue}", IconUrl = user.GetAvatarUrl() },
                        Timestamp = DateTimeOffset.UtcNow,
                        Fields =
                        {
                            new LocalEmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = true},
                            new LocalEmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = true},
                            new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = true}
                        }
                    };
                    var msg = await channel.SendMessageAsync(null, false, embed.Build());
                    caseId.MessageId = msg.Id.RawValue;
                    await db.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    _log.Log(NLog.LogLevel.Error, exception, $"(Log Service) Error in {guild.Id.RawValue} for UnBan Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task UserBanned(MemberBannedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.User;
                var guild = e.Guild;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                    if (!cfg.LogBan.HasValue) return;
                    var channel = guild.GetTextChannel(cfg.LogBan.Value);
                    if (channel == null) return;
                    
                    var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Ban);

                    CachedMember mod;
                    if (_cache.BanCache.TryGetValue(guild.Id.RawValue, out var cache))
                    {
                        if (cache.TryGetValue(user.Id.RawValue, out var result))
                        {
                            var modId = (ulong) result;
                            mod = guild.GetMember(modId);
                            if (mod != null)
                            {
                                caseId.ModId = mod.Id.RawValue;
                            }
                        }
                        else mod = await CheckAuditLog(guild, user.Id, caseId);
                    }
                    else mod = await CheckAuditLog(guild, user.Id, caseId);

                    var embed = new LocalEmbedBuilder
                    {
                        Color = Color.Red,
                        Author = new LocalEmbedAuthorBuilder {Name = $"User Banned | Case ID: {caseId.Id} | {user}"},
                        Fields =
                        {
                            new LocalEmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                            new LocalEmbedFieldBuilder {Name = "Moderator", Value = mod?.Mention ?? "N/A", IsInline = false},
                            new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false}
                        },
                        Footer = new LocalEmbedFooterBuilder {Text = $"User ID: {user.Id.RawValue}", IconUrl = user.GetAvatarUrl() },
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    if (user is CachedMember member)
                        embed.AddField("Time In Server", (DateTimeOffset.UtcNow - member.JoinedAt).Humanize(5));
                    
                    var msg = await channel.SendMessageAsync(null, false, embed.Build());
                    caseId.MessageId = msg.Id.RawValue;
                    await db.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    _log.Log(NLog.LogLevel.Error, exception, $"(Log Service) Error in {guild.Id.RawValue} for Ban Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private static async Task<CachedMember> CheckAuditLog(CachedGuild guild, Snowflake userId, ModLog caseId)
        {
            CachedMember mod = null;

            await Task.Delay(TimeSpan.FromSeconds(2));
            var audits = await guild.GetAuditLogsAsync<RestMemberBannedAuditLog>();
            var audit = audits.FirstOrDefault(x => x.TargetId.HasValue && x.TargetId.Value == userId);
            if (audit != null)
            {
                var temp = guild.GetMember(audit.ResponsibleUserId);
                if (!temp.IsBot) mod = temp;
                
                var reasonSplit = audit.Reason.Replace("(", " ").Replace(")", " ").Split(" ");
                var modId = FetchId(reasonSplit);
                if (modId.HasValue)
                {
                    temp = guild.GetMember(modId.Value);
                    if (temp != null && temp.Permissions.Contains(Permission.BanMembers) && !temp.IsBot) mod = temp;
                }
            }

            caseId.ModId = mod?.Id.RawValue;
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