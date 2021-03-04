using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task VoiceLog(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                var before = e.OldVoiceState;
                var after = e.NewVoiceState;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                    if (!cfg.LogVoice.HasValue) return;
                    var channel = user.Guild.GetTextChannel(cfg.LogVoice.Value);
                    if (channel == null) return;

                    var embed = new LocalEmbedBuilder
                    {
                        Color = _colourService.Get(user.Guild.Id.RawValue),
                        Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user} ({user.Id.RawValue})", IconUrl = user.GetAvatarUrl()}
                    };
                    // User muted, deafend or streaming
                    if (before != null && after != null && before.ChannelId == after.ChannelId)
                    {
                        if (before.IsDeafened && !after.IsDeafened)
                        {
                            // User undeafend
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Server Undeafend" };
                            embed.Description = $"{user} got server Undeafend in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsDeafened && after.IsDeafened)
                        {
                            // User deafend
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Server Deafend" };
                            embed.Description = $"{user} got server deafend in {user.VoiceChannel.Name}";
                        }
                        else if (before.IsMuted && !after.IsMuted)
                        {
                            // User unmuted
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Server Unmuted" };
                            embed.Description = $"{user} Unmuted in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsMuted && after.IsMuted)
                        {
                            // User muted
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Server Muted" };
                            embed.Description = $"{user} muted in {user.VoiceChannel.Name}";
                        }
                        // Self deafend
                        else if (before.IsSelfDeafened && !after.IsSelfDeafened)
                        {
                            // User Self undeafend
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Self Undeafened" };
                            embed.Description = $"{user} Undeafened in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsSelfDeafened && after.IsSelfDeafened)
                        {
                            // User Self deafend
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Self Deafened" };
                            embed.Description = $"{user} deafened in {user.VoiceChannel.Name}";
                        }
                        else if (before.IsSelfMuted && !after.IsSelfMuted)
                        {
                            // User Self unmuted
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Self Unmuted" };
                            embed.Description = $"{user} Unmuted in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsSelfMuted && after.IsSelfMuted)
                        {
                            // User Self muted
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Self Muted" };
                            embed.Description = $"{user} muted in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsStreaming && after.IsStreaming)
                        {
                            // User Started (game) Streaming
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Started Streaming(game)" };
                            embed.Description = $"{user} started streaming in {user.VoiceChannel.Name}";
                        }
                        else if (before.IsStreaming && !after.IsStreaming)
                        {
                            // User Stopped (game) Streaming
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Stopped Streaming(game)" };
                            embed.Description = $"{user} stopped streaming in {user.VoiceChannel.Name}";
                        }
                        else if (!before.IsVideoStreaming && after.IsVideoStreaming)
                        {
                            // User Started video (cam) streaming
                            embed.Author = new LocalEmbedAuthorBuilder { Name = "User Started Streaming(cam)" };
                            embed.Description = $"{user} started streaming in {user.VoiceChannel.Name}";
                        }
                        else if (before.IsVideoStreaming && !after.IsVideoStreaming)
                        {
                            // User Stopped video (cam) Streaming
                            embed.Author = new LocalEmbedAuthorBuilder {Name = "User Stopped Streaming(cam)"};
                            embed.Description = $"{user} stopped streaming in {user.VoiceChannel.Name}";
                        }
                        else return;
                    }
                    else if (before == null && after != null)
                    {
                        embed.Author = new LocalEmbedAuthorBuilder { Name = "Voice Channel Joined" };
                        embed.AddField("New Channel", user.Guild.GetVoiceChannel(e.NewVoiceState.ChannelId.RawValue).Name);
                    }
                    else if (before != null && after == null)
                    {
                        embed.Author = new LocalEmbedAuthorBuilder { Name = "Voice Channel Left" };
                        embed.AddField("Old Channel", user.Guild.GetVoiceChannel(before.ChannelId.RawValue).Name);
                    }
                    else if (before != null && before.ChannelId != after.ChannelId)
                    {
                        embed.Author = new LocalEmbedAuthorBuilder {Name = "Voice Channel Change"};
                        embed.AddField("Old Channel", user.Guild.GetVoiceChannel(before.ChannelId).Name);
                        embed.AddField("New Channel", user.Guild.GetVoiceChannel(after.ChannelId).Name);
                    }
                    else return;
                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception exception)
                {
                    _log.Log(NLog.LogLevel.Error, exception,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for Voice Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}