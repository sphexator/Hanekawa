using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.WebUI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.WebUI.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            var user = e.Member;
            var before = e.OldVoiceState;
            var after = e.NewVoiceState;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(user.GuildId);
            if (!cfg.LogVoice.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId);
            if (guild.GetChannel(cfg.LogVoice.Value) is not ITextChannel channel) return;

            var embed = new LocalEmbed
            {
                Color = _cache.GetColor(user.GuildId),
                Footer = new LocalEmbedFooter
                    {Text = $"Username: {user} ({user.Id})", IconUrl = user.GetAvatarUrl()}
            };
            // User muted, deafend or streaming
            if (before != null && after != null)
            {
                var oldVc = guild.GetChannel(before.ChannelId!.Value) as IVoiceChannel;
                var newVc = guild.GetChannel(after.ChannelId!.Value) as IVoiceChannel;

                if (before.IsDeafened && !after.IsDeafened)
                {
                    // User undeafend
                    embed.Author = new LocalEmbedAuthor {Name = "User Server Undeafend"};
                    embed.Description = $"{user} got server Undeafend in {oldVc.Name}";
                }
                else if (!before.IsDeafened && after.IsDeafened)
                {
                    // User deafend
                    embed.Author = new LocalEmbedAuthor {Name = "User Server Deafend"};
                    embed.Description = $"{user} got server deafend in {oldVc.Name}";
                }
                else if (before.IsMuted && !after.IsMuted)
                {
                    // User unmuted
                    embed.Author = new LocalEmbedAuthor {Name = "User Server Unmuted"};
                    embed.Description = $"{user} Unmuted in {oldVc.Name}";
                }
                else if (!before.IsMuted && after.IsMuted)
                {
                    // User muted
                    embed.Author = new LocalEmbedAuthor {Name = "User Server Muted"};
                    embed.Description = $"{user} muted in {oldVc.Name}";
                }
                else if (before.IsSelfDeafened && !after.IsSelfDeafened)
                {
                    // User Self undeafend
                    embed.Author = new LocalEmbedAuthor {Name = "User Self Undeafened"};
                    embed.Description = $"{user} Undeafened in {oldVc.Name}";
                }
                else if (!before.IsSelfDeafened && after.IsSelfDeafened)
                {
                    // User Self deafend
                    embed.Author = new LocalEmbedAuthor {Name = "User Self Deafened"};
                    embed.Description = $"{user} deafened in {oldVc.Name}";
                }
                else if (before.IsSelfMuted && !after.IsSelfMuted)
                {
                    // User Self unmuted
                    embed.Author = new LocalEmbedAuthor {Name = "User Self Unmuted"};
                    embed.Description = $"{user} Unmuted in {oldVc.Name}";
                }
                else if (!before.IsSelfMuted && after.IsSelfMuted)
                {
                    // User Self muted
                    embed.Author = new LocalEmbedAuthor {Name = "User Self Muted"};
                    embed.Description = $"{user} muted in {oldVc.Name}";
                }
                else if (!before.IsStreaming && after.IsStreaming)
                {
                    // User Started (game) Streaming
                    embed.Author = new LocalEmbedAuthor {Name = "User Started Streaming(game)"};
                    embed.Description = $"{user} started streaming in {oldVc.Name}";
                }
                else if (before.IsStreaming && !after.IsStreaming)
                {
                    // User Stopped (game) Streaming
                    embed.Author = new LocalEmbedAuthor {Name = "User Stopped Streaming(game)"};
                    embed.Description = $"{user} stopped streaming in {oldVc.Name}";
                }
                else if (!before.IsTransmittingVideo && after.IsTransmittingVideo)
                {
                    // User Started video (cam) streaming
                    embed.Author = new LocalEmbedAuthor {Name = "User Started Streaming(cam)"};
                    embed.Description = $"{user} started streaming in {oldVc.Name}";
                }
                else if (before.IsTransmittingVideo && !after.IsTransmittingVideo)
                {
                    // User Started video (cam) streaming
                    embed.Author = new LocalEmbedAuthor {Name = "User Stopped Streaming(cam)"};
                    embed.Description = $"{user} Stopped streaming in {oldVc.Name}";
                }
                else if (before.ChannelId.Value != after.ChannelId.Value)
                {
                    // User changed VC
                    embed.Author = new LocalEmbedAuthor {Name = "Voice Channel Change"};
                    embed.AddField("Old Channel", oldVc.Name);
                    embed.AddField("New Channel", newVc.Name);
                }
                else return;
            }
            else if (before == null && after.ChannelId.HasValue)
            {
                var newVc = guild.GetChannel(after.ChannelId.Value) as IVoiceChannel;
                embed.Author = new LocalEmbedAuthor {Name = "Voice Channel Joined"};
                embed.AddField("New Channel", newVc.Name);
            }
            else if (after == null && before.ChannelId.HasValue)
            {
                var oldVc = guild.GetChannel(before.ChannelId.Value) as IVoiceChannel;
                embed.Author = new LocalEmbedAuthor {Name = "Voice Channel Left"};
                embed.AddField("Old Channel", oldVc.Name);
            }
            else return;

            var builder = new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed> {embed},
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false,
                Name = guild.Name,
                AvatarUrl = guild.GetIconUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookVoiceId.Value, cfg.WebhookVoice);
                await webhook.ExecuteAsync(builder);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for voice log, re-creating");
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookVoice != webhook.Token) cfg.WebhookVoice = webhook.Token;
                if (!cfg.WebhookVoiceId.HasValue || cfg.WebhookVoiceId.Value != webhook.Id)
                    cfg.WebhookVoiceId = webhook.Id;
                await webhook.ExecuteAsync(builder);
                await db.SaveChangesAsync();
            }
        }
    }
}