using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.AuditLogs;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Entities.Moderation;
using Hanekawa.WebUI.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.WebUI.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnBanCreated(BanCreatedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if (guild == null) return;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(guild.Id);
                if (!cfg.LogBan.HasValue) return;
                if(guild.GetChannel(cfg.LogBan.Value) is not ITextChannel channel) return;

                var caseId = await db.CreateIncrementEntityAsync<ModLog>(e.GuildId, e.UserId);
                caseId.Action = nameof(ModAction.Ban);
                IMember mod;
                var banCacheUser = _cache.TryGetBanCache(guild.Id, e.UserId);
                if (banCacheUser != null)
                {
                    mod = await _bot.GetOrFetchMemberAsync(e.GuildId, banCacheUser.Value) ??
                          await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberBanned);
                    if (mod != null) caseId.ModId = mod.Id;
                }
                else mod = await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberBanned);
                
                var embed = new LocalEmbed
                {
                    Color = HanaBaseColor.Red(),
                    Author = new LocalEmbedAuthor { Name = $"User Banned | Case ID: {caseId.Id} | {e.User}" },
                    Fields =
                    {
                        new LocalEmbedField {Name = "User", Value = e.User.Mention, IsInline = false},
                        new LocalEmbedField {Name = "Moderator", Value = mod?.Mention ?? "N/A", IsInline = false},
                        new LocalEmbedField {Name = "Reason", Value = "N/A", IsInline = false}
                    },
                    Footer = new LocalEmbedFooter { Text = $"User ID: {e.User.Id}", IconUrl = e.User.GetAvatarUrl() },
                    Timestamp = DateTimeOffset.UtcNow
                };
                if (e.User is CachedMember {JoinedAt: {HasValue: true}} member)
                    embed.AddField("Time In Server", (DateTimeOffset.UtcNow - member.JoinedAt.Value).Humanize(5));
                
                var builder = new LocalWebhookMessage
                {
                    Embeds = new List<LocalEmbed> { embed },
                    IsTextToSpeech = false,
                    AllowedMentions = LocalAllowedMentions.None,
                    Name = guild.Name,
                    AvatarUrl = guild.GetIconUrl()
                };
                try
                {
                    var webhook = _webhookClientFactory.CreateClient(cfg.WebhookBanId.Value, cfg.WebhookBan);
                    var msg = await webhook.ExecuteAsync(builder);
                    caseId.MessageId = msg.Id;
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Warn, exception, $"No valid webhook for ban, re-creating");
                    var webhook = await channel.GetOrCreateWebhookClientAsync();
                    cfg.WebhookBan = webhook.Token;
                    cfg.WebhookBanId = webhook.Id;
                    var msg = await webhook.ExecuteAsync(builder);
                    caseId.MessageId = msg.Id;
                }
                finally
                {
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Error in {guild.Id} for Ban Log - {exception.Message}");
            }
        }

        protected override async ValueTask OnBanDeleted(BanDeletedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if (guild == null) return;

            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(guild.Id);
                if (!cfg.LogBan.HasValue) return;
                if(guild.GetChannel(cfg.LogBan.Value) is not CachedTextChannel channel) return;
                var caseId = await db.CreateIncrementEntityAsync<ModLog>(e.GuildId, e.UserId);
                caseId.Action = nameof(ModAction.Unban);
                
                IMember mod;
                var banCacheUser = _cache.TryGetBanCache(guild.Id, e.UserId);
                if (banCacheUser != null)
                {
                    mod = await _bot.GetOrFetchMemberAsync(e.GuildId, banCacheUser.Value) ??
                          await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberUnbanned);
                    if (mod != null) caseId.ModId = mod.Id;
                }
                else mod = await CheckAuditLog(guild, e.User.Id, caseId, AuditLogActionType.MemberUnbanned);

                var builder = new LocalWebhookMessage
                {
                    Embeds = new List<LocalEmbed>
                    {
                        new()
                        {
                            Color = HanaBaseColor.Ok(),
                            Author = new ()
                                {Name = $"User Unbanned | Case ID: {caseId.Id} | {e.User}"},
                            Footer = new()
                                {Text = $"User ID: {e.User.Id}", IconUrl = e.User.GetAvatarUrl()},
                            Timestamp = DateTimeOffset.UtcNow,
                            Fields =
                            {
                                new LocalEmbedField {Name = "User", Value = $"{e.User.Mention}", IsInline = true},
                                new LocalEmbedField {Name = "Moderator", Value = mod?.Mention ?? "N/A", IsInline = true},
                                new LocalEmbedField {Name = "Reason", Value = "N/A", IsInline = true}
                            }
                        }
                    },
                    IsTextToSpeech = false,
                    AllowedMentions = LocalAllowedMentions.None,
                    Name = guild.Name,
                    AvatarUrl = guild.GetIconUrl()
                };
                try
                {
                    var webhook = _webhookClientFactory.CreateClient(cfg.WebhookBanId.Value, cfg.WebhookBan);
                    var msg = await webhook.ExecuteAsync(builder);
                    caseId.MessageId = msg.Id;
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Warn, exception, $"No valid webhook for unban, re-creating");
                    var webhook = await channel.GetOrCreateWebhookClientAsync();
                    cfg.WebhookBan = webhook.Token;
                    cfg.WebhookBanId = webhook.Id;
                    var msg = await webhook.ExecuteAsync(builder);
                    caseId.MessageId = msg.Id;
                }
                finally
                {
                    await db.SaveChangesAsync();   
                }
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Error in {e.GuildId} for UnBan Log - {exception.Message}");
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
                    audits = await guild.FetchAuditLogsAsync<IMemberBannedAuditLog>();
                    break;
                case AuditLogActionType.MemberUnbanned:
                    audits = await guild.FetchAuditLogsAsync<IMemberUnbannedAuditLog>();
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