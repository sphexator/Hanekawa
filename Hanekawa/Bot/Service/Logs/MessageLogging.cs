﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz.Util;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId.Value);
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
            if (!cfg.LogMsg.HasValue) return;
            if (guild.GetChannel(cfg.LogMsg.Value) is not ITextChannel channel) return;
            if (e.Message == null || e.Message.Author.IsBot) return;
            var tempChannel = guild.GetChannel(e.ChannelId);
            if (tempChannel == null) return;
            var msgChannel = tempChannel as CachedTextChannel;
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {Name = "Message Deleted"},
                Color = _cache.GetColor(guild.Id),
                Timestamp = e.Message.CreatedAt,
                Description = $"{e.Message.Author.Mention} deleted a message in {tempChannel.Name}",
                Footer = new LocalEmbedFooterBuilder
                {
                    Text = $"User: {e.Message.Author.Id} | Message ID: {e.MessageId}",
                    IconUrl = e.Message.Author.GetAvatarUrl()
                }
            };
            if (!e.Message.Content.IsNullOrWhiteSpace())
                embed.AddField("Content", e.Message.Content.Truncate(1499));

            if (msgChannel != null && e.Message.Attachments.Count > 0 && !msgChannel.IsNsfw)
            {
                var file = e.Message.Attachments.FirstOrDefault();
                if (file != null)
                    embed.AddField(x =>
                    {
                        x.Name = "File";
                        x.IsInline = false;
                        x.Value = file.ProxyUrl;
                    });
            }

            var builder = new LocalWebhookMessageBuilder
            {
                Embeds = new List<LocalEmbedBuilder> {embed},
                Mentions = LocalMentionsBuilder.None,
                IsTextToSpeech = false,
                Name = guild.GetCurrentUser().DisplayName(),
                AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookMessageId.Value, cfg.WebhookMessage);
                await webhook.ExecuteAsync(builder.Build());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for message deleted, re-creating");
                var webhook = await channel.GetOrCreateWebhookClient();
                if (cfg.WebhookMessage != webhook.Token) cfg.WebhookMessage = webhook.Token;
                if (!cfg.WebhookMessageId.HasValue || cfg.WebhookMessageId.Value != webhook.Id)
                    cfg.WebhookMessageId = webhook.Id;
                await webhook.ExecuteAsync(builder.Build());
                await db.SaveChangesAsync();
            }
        }

        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(guild.Id);
            if (!cfg.LogMsg.HasValue) return;
            if (guild.GetChannel(cfg.LogMsg.Value) is not ITextChannel channel) return;
            var tempChannel = guild.GetChannel(e.ChannelId);
            if (tempChannel == null) return;

            var messageContent = new List<string>();
            var content = new StringBuilder();
            foreach (var (_, cachedUserMessage) in e.Messages)
            {
                if (!cachedUserMessage.Author.IsBot) continue;
                var user = cachedUserMessage.Author;
                if (content.Length + cachedUserMessage.Content.Length >= 1950)
                {
                    messageContent.Add(content.ToString());
                    content.Clear();
                }

                content.AppendLine($"{user}: {cachedUserMessage.Content}");
            }

            if (content.Length > 0) messageContent.Add(content.ToString());
            var embeds = new List<LocalEmbedBuilder>();
            foreach (var text in messageContent)
            {
                embeds.Add(new LocalEmbedBuilder
                {
                    Color = _cache.GetColor(guild.Id),
                    Author = new LocalEmbedAuthorBuilder {Name = $"Bulk delete in {tempChannel.Name}"},
                    Description = text
                });
            }

            var builder = new LocalWebhookMessageBuilder
            {
                Embeds = embeds,
                Mentions = LocalMentionsBuilder.None,
                IsTextToSpeech = false,
                Name = guild.GetCurrentUser().DisplayName(),
                AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookMessageId.Value, cfg.WebhookMessage);
                await webhook.ExecuteAsync(builder.Build());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for member left, re-creating");
                var webhook = await channel.GetOrCreateWebhookClient();
                if (cfg.WebhookMessage != webhook.Token) cfg.WebhookMessage = webhook.Token;
                if (!cfg.WebhookMessageId.HasValue || cfg.WebhookMessageId.Value != webhook.Id)
                    cfg.WebhookMessageId = webhook.Id;
                await webhook.ExecuteAsync(builder.Build());
                await db.SaveChangesAsync();
            }
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            if(!e.GuildId.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId.Value);
            var before = e.OldMessage;
            var after = e.NewMessage;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(e.GuildId.Value);
            if (!cfg.LogMsg.HasValue) return;
            if (guild.GetChannel(cfg.LogMsg.Value) is not ITextChannel channel) return;
            if (before != null && before.Content == after.Content) return;

            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {Name = "Message Updated"},
                Color = _cache.GetColor(e.GuildId.Value),
                Timestamp = after.EditedAt ?? after.CreatedAt,
                Description =
                    $"{after.Author.Mention} updated a message in {_bot.GetChannel(guild.Id, e.ChannelId).Name}",
                Footer = new LocalEmbedFooterBuilder
                {
                    Text = $"User: {after.Author.Id} | {after.Id}",
                    IconUrl = after.Author.GetAvatarUrl()
                }
            };
            embed.AddField("Updated Message", after.Content.Truncate(980));
            embed.AddField("Old Message",
                before != null ? before.Content.Truncate(980) : "Unknown - Message not in cache");
            var builder = new LocalWebhookMessageBuilder
            {
                Embeds = new List<LocalEmbedBuilder> {embed},
                Mentions = LocalMentionsBuilder.None,
                IsTextToSpeech = false,
                Name = guild.GetCurrentUser().DisplayName(),
                AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookMessageId.Value, cfg.WebhookMessage);
                await webhook.ExecuteAsync(builder.Build());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for message updated, re-creating");
                var webhook = await channel.GetOrCreateWebhookClient();
                if (cfg.WebhookMessage != webhook.Token) cfg.WebhookMessage = webhook.Token;
                if (!cfg.WebhookMessageId.HasValue || cfg.WebhookMessageId.Value != webhook.Id)
                    cfg.WebhookMessageId = webhook.Id;
                await webhook.ExecuteAsync(builder.Build());
                await db.SaveChangesAsync();
            }
        }
    }
}