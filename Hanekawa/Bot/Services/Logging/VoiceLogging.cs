using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task VoiceLog(SocketUser usr, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if (!(usr is SocketGuildUser user)) return;
                var cfg = await _db.GetOrCreateLoggingConfigAsync(user.Guild);
                if (!cfg.LogAvi.HasValue) return;
                var channel = user.Guild.GetTextChannel(cfg.LogAvi.Value);
                if (channel == null) return;
                var embed = new EmbedBuilder().CreateDefault(null, user.Guild.Id);
                embed.Footer = new EmbedFooterBuilder { Text = $"Username: {user} ({user.Id})"};
                if ((before.IsDeafened || before.IsSelfDeafened) != (after.IsDeafened || after.IsSelfDeafened))
                {
                    embed.Author = new EmbedAuthorBuilder { Name = "User Deafened"};
                    embed.Description = $"{user} deafened in {after.VoiceChannel.Name}";
                }
                
                if ((before.IsMuted || before.IsSelfMuted) != (after.IsMuted || after.IsSelfMuted))
                {
                    embed.Author = new EmbedAuthorBuilder { Name = "User Muted"};
                    embed.Description = $"{user} muted in {after.VoiceChannel.Name}";
                }

                if (before.VoiceChannel != after.VoiceChannel)
                {
                    embed.Author = new EmbedAuthorBuilder { Name = "Voice Channel Change" };
                    if (before.VoiceChannel == null)
                    {
                        embed.Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder { Name = "Old Channel", Value = "N/A", IsInline = false},
                            new EmbedFieldBuilder { Name = "New Channel", Value = after.VoiceChannel.Name, IsInline = false }
                        };
                    }

                    if (after.VoiceChannel == null)
                    {
                        embed.Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder { Name = "Old Channel", Value = after.VoiceChannel.Name, IsInline = false},
                            new EmbedFieldBuilder { Name = "New Channel", Value = "N/A", IsInline = false }
                        };
                    }
                    else
                    {
                        embed.Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder { Name = "Old Channel", Value = after.VoiceChannel.Name, IsInline = false},
                            new EmbedFieldBuilder { Name = "New Channel", Value = after.VoiceChannel.Name, IsInline = false }
                        };
                    }
                }

                await channel.SendMessageAsync(null, false, embed.Build());
            });
            return Task.CompletedTask;
        }
    }
}