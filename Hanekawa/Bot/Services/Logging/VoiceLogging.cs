using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
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
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                        if (!cfg.LogVoice.HasValue) return;
                        var channel = user.Guild.GetTextChannel(cfg.LogVoice.Value);
                        if (channel == null) return;

                        var embed = new LocalEmbedBuilder {Color = _colourService.Get(user.Guild.Id)};
                        embed.Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"};
                        if ((before.IsDeafened || before.IsSelfDeafened) != (after.IsDeafened || after.IsSelfDeafened))
                        {
                            embed.Author = new LocalEmbedAuthorBuilder {Name = "User Deafened"};
                            embed.Description = $"{user} deafened in {user.VoiceChannel.Name}";
                        }

                        if ((before.IsMuted || before.IsSelfMuted) != (after.IsMuted || after.IsSelfMuted))
                        {
                            embed.Author = new LocalEmbedAuthorBuilder {Name = "User Muted"};
                            embed.Description = $"{user} muted in {user.VoiceChannel.Name}";
                        }

                        if (before.ChannelId.RawValue != after.ChannelId.RawValue)
                        {
                            embed.Author = new LocalEmbedAuthorBuilder {Name = "Voice Channel Change"};
                            if (before == null)
                            {
                                embed.AddField("Old Channel", "N/A");
                                embed.AddField("New Channel", user.Guild.GetTextChannel(after.ChannelId).Name);
                            }

                            if (after == null)
                            {
                                embed.AddField("Old Channel", user.Guild.GetVoiceChannel(before.ChannelId).Name);
                                embed.AddField("New Channel", "N/A");
                            }
                            else
                            {
                                embed.AddField("Old Channel", user.Guild.GetVoiceChannel(before.ChannelId).Name);
                                embed.AddField("New Channel", user.Guild.GetVoiceChannel(after.ChannelId).Name);
                            }
                        }

                        await channel.SendMessageAsync(null, false, embed.Build());
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id} for Voice Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}