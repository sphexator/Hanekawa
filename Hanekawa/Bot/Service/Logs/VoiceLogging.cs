using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            var user = e.Member;
            var before = e.OldVoiceState;
            var after = e.NewVoiceState;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(user.GuildId);
                if (!cfg.LogVoice.HasValue) return;
                var guild = _bot.GetGuild(e.GuildId);
                var channel = guild.GetChannel(cfg.LogVoice.Value);
                if (channel == null) return;

                var embed = new LocalEmbedBuilder
                {
                    Color = _cache.GetColor(user.GuildId.RawValue),
                    Footer = new LocalEmbedFooterBuilder
                        {Text = $"Username: {user} ({user.Id.RawValue})", IconUrl = user.GetAvatarUrl()}
                };
                // User muted, deafend or streaming
                if (before != null && after != null)
                {
                    var oldVc = guild.GetChannel(before.ChannelId!.Value) as IVoiceChannel;
                    var newVc = guild.GetChannel(after.ChannelId!.Value) as IVoiceChannel;

                    if (before.IsDeafened && !after.IsDeafened)
                    {
                        // User undeafend
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Server Undeafend"};
                        embed.Description = $"{user} got server Undeafend in {oldVc.Name}";
                    }
                    else if (!before.IsDeafened && after.IsDeafened)
                    {
                        // User deafend
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Server Deafend"};
                        embed.Description = $"{user} got server deafend in {oldVc.Name}";
                    }
                    else if (before.IsMuted && !after.IsMuted)
                    {
                        // User unmuted
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Server Unmuted"};
                        embed.Description = $"{user} Unmuted in {oldVc.Name}";
                    }
                    else if (!before.IsMuted && after.IsMuted)
                    {
                        // User muted
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Server Muted"};
                        embed.Description = $"{user} muted in {oldVc.Name}";
                    }
                    else if (before.IsSelfDeafened && !after.IsSelfDeafened)
                    {
                        // User Self undeafend
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Self Undeafened"};
                        embed.Description = $"{user} Undeafened in {oldVc.Name}";
                    }
                    else if (!before.IsSelfDeafened && after.IsSelfDeafened)
                    {
                        // User Self deafend
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Self Deafened"};
                        embed.Description = $"{user} deafened in {oldVc.Name}";
                    }
                    else if (before.IsSelfMuted && !after.IsSelfMuted)
                    {
                        // User Self unmuted
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Self Unmuted"};
                        embed.Description = $"{user} Unmuted in {oldVc.Name}";
                    }
                    else if (!before.IsSelfMuted && after.IsSelfMuted)
                    {
                        // User Self muted
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Self Muted"};
                        embed.Description = $"{user} muted in {oldVc.Name}";
                    }
                    else if (!before.IsStreaming && after.IsStreaming)
                    {
                        // User Started (game) Streaming
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Started Streaming(game)"};
                        embed.Description = $"{user} started streaming in {oldVc.Name}";
                    }
                    else if (before.IsStreaming && !after.IsStreaming)
                    {
                        // User Stopped (game) Streaming
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Stopped Streaming(game)"};
                        embed.Description = $"{user} stopped streaming in {oldVc.Name}";
                    }
                    else if (!before.IsTransmittingVideo && after.IsTransmittingVideo)
                    {
                        // User Started video (cam) streaming
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Started Streaming(cam)"};
                        embed.Description = $"{user} started streaming in {oldVc.Name}";
                    }
                    else if (before.IsTransmittingVideo && !after.IsTransmittingVideo)
                    {
                        // User Started video (cam) streaming
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "User Stopped Streaming(cam)"};
                        embed.Description = $"{user} Stopped streaming in {oldVc.Name}";
                    }
                    else if (before.ChannelId.Value != after.ChannelId.Value)
                    {
                        // User changed VC
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "Voice Channel Change"};
                        embed.AddField("Old Channel", oldVc.Name);
                        embed.AddField("New Channel", newVc.Name);
                    }
                    else return;
                }
                else if (before == null && after.ChannelId.HasValue)
                {
                    var newVc = guild.GetChannel(after.ChannelId.Value) as IVoiceChannel;
                    embed.Author = new LocalEmbedAuthorBuilder {Name = "Voice Channel Joined"};
                    embed.AddField("New Channel", newVc.Name);
                }
                else if (after == null && before.ChannelId.HasValue)
                {
                    var oldVc = guild.GetChannel(before.ChannelId.Value) as IVoiceChannel;
                    embed.Author = new LocalEmbedAuthorBuilder {Name = "Voice Channel Left"};
                    embed.AddField("Old Channel", oldVc.Name);
                }
                else return;

                await (channel as ITextChannel).SendMessageAsync(new LocalMessageBuilder
                {
                    Attachments = null,
                    Embed = embed,
                    Content = null,
                    Mentions = LocalMentionsBuilder.None,
                    IsTextToSpeech = false,
                }.Build());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {user.GuildId.RawValue} for Voice Log - {exception.Message}");
            }
        }
    }
}