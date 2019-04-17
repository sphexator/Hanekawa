using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database.Extensions;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel ch)
        {
            _ = Task.Run(async () =>
            {
                if (!(after.Author is SocketGuildUser user)) return;
                var cfg = await _db.GetOrCreateLoggingConfigAsync(user.Guild);
                if (!cfg.LogMsg.HasValue) return;
                var channel = user.Guild.GetTextChannel(cfg.LogMsg.Value);
                if (channel == null) return;

                // TODO: Add this
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel ch)
        {
            _ = Task.Run(async () =>
            {
                if (!(ch is ITextChannel chx)) return;
                var cfg = await _db.GetOrCreateLoggingConfigAsync(chx.Guild);
                if (!cfg.LogMsg.HasValue) return;
                var channel = await chx.Guild.GetTextChannelAsync(cfg.LogMsg.Value);
                if (channel == null) return;

                if (!message.HasValue) await message.GetOrDownloadAsync();

                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = new EmbedAuthorBuilder { Name = "Message Deleted"},
                    Description = $"{message.Value.Author.Mention} deleted a message in {chx.Mention}",
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new EmbedFooterBuilder { Text = $"User: {message.Value.Author.Id} | Message ID: {message.Value.Id}"},
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder {Name = "Message", Value = message.Value.Content.Truncate(900), IsInline = false }
                    }
                };

                if (message.Value.Attachments.Count > 0 && !chx.IsNsfw)
                {
                    var file = message.Value.Attachments.FirstOrDefault();
                    if (file != null)
                        embed.AddField(x =>
                        {
                            x.Name = "File";
                            x.IsInline = false;
                            x.Value = message.Value.Attachments.FirstOrDefault()?.Url;
                        });
                }

                await channel.SendMessageAsync(null, false, embed.Build());
            });
            return Task.CompletedTask;
        }
    }
}
