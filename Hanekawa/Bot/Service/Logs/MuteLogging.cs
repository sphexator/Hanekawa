using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        public async ValueTask MuteAsync(IMember target, IMember staff, string reason, DbService db, TimeSpan? duration = null)
        {
            var guild = _bot.GetGuild(target.GuildId);
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(target.GuildId);
            if (!cfg.LogBan.HasValue) return;
            if (guild.GetChannel(cfg.LogBan.Value) is not ITextChannel channel) return;
            var caseId = await db.CreateIncrementEntityAsync<ModLog>(staff.GuildId, target.Id);
            caseId.Action = nameof(ModAction.Mute);
            var embed = new LocalEmbed
            {
                Author = new LocalEmbedAuthor { Name = $"Case ID: {caseId.Id} - User Muted | {target}" },
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Fields =
                {
                    new LocalEmbedField {Name = "User", Value = target.Mention, IsInline = false},
                    new LocalEmbedField {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedField {Name = "Reason", Value = reason, IsInline = false}
                },
                Footer = new LocalEmbedFooter { Text = $"Username: {target} ({target.Id})", IconUrl = target.GetAvatarUrl() }
            };
            if (duration.HasValue)
                embed.Fields.Add(new LocalEmbedField
                    {Name = "Duration", Value = $"{duration.Value.Humanize(2)}", IsInline = false});
            var builder = new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed>{embed},
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false,
                Name = guild.Name,
                AvatarUrl = guild.GetIconUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookBanId.Value, cfg.WebhookBan);
                var msg = await webhook.ExecuteAsync(builder);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"No valid webhook for mute log, re-creating");
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookBan != webhook.Token) cfg.WebhookBan = webhook.Token;
                if (!cfg.WebhookBanId.HasValue || cfg.WebhookBanId.Value != webhook.Id)
                    cfg.WebhookBanId = webhook.Id;
                var msg = await webhook.ExecuteAsync(builder);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
            }
        }

        public async ValueTask WarnAsync(WarnReason warn, IMember target, IMember staff, string reason, DbService db, TimeSpan? duration = null)
        {
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(target.GuildId);
            if (!cfg.LogWarn.HasValue) return;
            var guild = _bot.GetGuild(target.GuildId);
            if (guild.GetChannel(cfg.LogWarn.Value) is not ITextChannel channel) return;
            
            var embed = new LocalEmbed
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthor { Name = $"User {warn}" },
                Footer = new LocalEmbedFooter { Text = $"Username: {target} ({target.Id})" },
                Fields =
                {
                    new LocalEmbedField {Name = "User", Value = target.Mention, IsInline = false},
                    new LocalEmbedField {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedField {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            if (duration.HasValue)
                embed.Fields.Add(new LocalEmbedField
                    { Name = "Duration", Value = $"{duration.Value.Humanize(2)}", IsInline = false });
            var builder = new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed>{embed},
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false,
                Name = guild.Name,
                AvatarUrl = guild.GetIconUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookWarnId.Value, cfg.WebhookWarn);
                await webhook.ExecuteAsync(builder);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"No valid webhook for warn log, re-creating");
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookWarn != webhook.Token) cfg.WebhookWarn = webhook.Token;
                if (!cfg.WebhookWarnId.HasValue || cfg.WebhookWarnId.Value != webhook.Id)
                    cfg.WebhookWarnId = webhook.Id;
                await webhook.ExecuteAsync(builder);
                await db.SaveChangesAsync();
            }
        }
    }
}
