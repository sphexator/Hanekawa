using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
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
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogMsg.HasValue) return;
                var channel = guild.GetChannel(cfg.LogMsg.Value);
                if (channel == null) return;
                if (e.Message == null || e.Message.Author.IsBot) return;
                var tempChannel = guild.GetChannel(e.ChannelId);
                if (tempChannel == null) return;
                var msgChannel = tempChannel as CachedTextChannel;
                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder { Name = "Message Deleted" },
                    Color = _cache.GetColor(guild.Id),
                    Timestamp = e.Message.CreatedAt,
                    Description = $"{e.Message.Author.Mention} deleted a message in {tempChannel.Name}",
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"User: {e.Message.Author.Id.RawValue} | Message ID: {e.MessageId.RawValue}",
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

                await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    IsTextToSpeech = false,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    Attachments = null,
                    Content = null
                }.Build());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {guild.Id.RawValue} for Message Deleted - {exception.Message}");
            }
        }

        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild.Id.RawValue);
                if (!cfg.LogMsg.HasValue) return;
                var channel = guild.GetChannel(cfg.LogMsg.Value);
                if (channel == null) return;
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

                foreach (var embed in messageContent.Select(t => new LocalEmbedBuilder
                {
                    Color = _cache.GetColor(guild.Id),
                    Title = $"Bulk delete in {tempChannel.Name}",
                    Description = t
                }))
                {
                    await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                    {
                        Embed = embed,
                        IsTextToSpeech = false,
                        Mentions = LocalMentionsBuilder.None,
                        Reference = null,
                        Attachments = null,
                        Content = null
                    }.Build());
                    await Task.Delay(1000);
                }
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {guild.Id.RawValue} for Bulk Message Deleted - {exception.Message}");
            }
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            if(!e.GuildId.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId.Value);
            var before = e.OldMessage;
            var after = e.NewMessage;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(e.GuildId.Value.RawValue);
                if (!cfg.LogMsg.HasValue) return;
                var channel = guild.GetChannel(cfg.LogMsg.Value);
                if (channel == null) return;
                if (before != null && before.Content == after.Content) return;

                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder { Name = "Message Updated" },
                    Color = _cache.GetColor(e.GuildId.Value),
                    Timestamp = after.EditedAt ?? after.CreatedAt,
                    Description = $"{after.Author.Mention} updated a message in {_bot.GetChannel(guild.Id, e.ChannelId).Name}",
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"User: {after.Author.Id.RawValue} | {after.Id.RawValue}",
                        IconUrl = after.Author.GetAvatarUrl()
                    }
                };
                embed.AddField("Updated Message", after.Content.Truncate(980));
                embed.AddField("Old Message",
                    before != null ? before.Content.Truncate(980) : "Unknown - Message not in cache");
                await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    IsTextToSpeech = false,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    Attachments = null,
                    Content = null
                }.Build());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {guild.Id.RawValue} for Message Updated - {exception.Message}");
            }
        }
    }
}