using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Moderation;
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
                if(!guild.Channels.TryGetValue(cfg.LogBan.Value, out var channel)) return;

                var caseId = await db.CreateCaseId(e.User, guild, DateTime.UtcNow, ModAction.Ban);
                CachedMember mod;
                if (_cache.BanCache.TryGetValue(guild.Id.RawValue, out var cache))
                {
                    if (cache.TryGetValue(e.User.Id.RawValue, out var result))
                    {
                        var modId = (ulong)result;
                        mod = _bot.GetMember(e.GuildId, modId);
                        if (mod != null)
                        {
                            caseId.ModId = mod.Id.RawValue;
                        }
                    }
                    else mod = await CheckAuditLog(guild, e.User.Id, caseId);
                }
                else mod = await CheckAuditLog(guild, e.User.Id, caseId);

                var embed = new LocalEmbedBuilder
                {
                    Color = Color.Red,
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
                if (e.User is CachedMember member && member.JoinedAt.HasValue)
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
                caseId.MessageId = msg.Id.RawValue;
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
                if (!guild.Channels.TryGetValue(cfg.LogBan.Value, out var channel)) return;
                var caseId = await db.CreateCaseId(e.User, guild, DateTime.UtcNow, ModAction.Unban);
                var embed = new LocalEmbedBuilder
                {
                    Color = Color.Green,
                    Author = new LocalEmbedAuthorBuilder { Name = $"User Unbanned | Case ID: {caseId.Id} | {e.User}" },
                    Footer = new LocalEmbedFooterBuilder { Text = $"User ID: {e.User.Id.RawValue}", IconUrl = e.User.GetAvatarUrl() },
                    Timestamp = DateTimeOffset.UtcNow,
                    Fields =
                    {
                        new LocalEmbedFieldBuilder {Name = "User", Value = $"{e.User.Mention}", IsInline = true},
                        new LocalEmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = true},
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

        private async Task<IMember> CheckAuditLog(CachedGuild guild, Snowflake userId, ModLog caseId)
        {
            IMember mod = null;

            await Task.Delay(TimeSpan.FromSeconds(2));
            var audits = await guild.GetAuditLogsAsync<RestMemberBannedAuditLog>(); //TODO: Whenever audit log is implemented
            var audit = audits.FirstOrDefault(x => x.TargetId.HasValue && x.TargetId.Value == userId);
            if (audit != null)
            {
                var temp = _bot.GetMember(guild.Id, audit.ResponsibleUserId);
                if (!temp.IsBot) mod = temp;

                var reasonSplit = audit.Reason.Replace("(", " ").Replace(")", " ").Replace("-", " ").Split(" ");
                var modId = FetchId(reasonSplit);
                if (modId.HasValue)
                {
                    temp = _bot.GetMember(guild.Id, modId.Value);
                    if (temp != null && !temp.IsBot && Discord.Permissions
                        .CalculatePermissions(guild, temp, temp.GetRoles().Values).BanMembers) mod = temp;
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
