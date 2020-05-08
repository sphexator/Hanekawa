using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task MessageUpdated(MessageUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var ch = e.Channel;
                var before = e.OldMessage;
                var after = e.NewMessage;
                if (!(after.Author is CachedMember user)) return;
                if (user.IsBot) return;
                if (!(ch is ITextChannel chx)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                    if (!cfg.LogMsg.HasValue) return;
                    var channel = user.Guild.GetTextChannel(cfg.LogMsg.Value);
                    if (channel == null) return;

                    if (before.Value.Content == null) return;
                    if (before.Value.Content == after.Content) return;

                    var embed = new LocalEmbedBuilder
                    {
                        Description = $"{user.Mention} updated a message in {chx.Mention}",
                        Color = _colourService.Get(user.Guild.Id.RawValue),
                        Author = new LocalEmbedAuthorBuilder { Name = "Message Updated" },
                        Fields =
                        {
                            new LocalEmbedFieldBuilder
                                {Name = "Updated Message", Value = after.Content.Truncate(980), IsInline = true},
                            new LocalEmbedFieldBuilder
                                {Name = "Old Message", Value = before.Value.Content.Truncate(980), IsInline = true}
                        },
                        Footer = new LocalEmbedFooterBuilder {Text = $"User: {user.Id.RawValue} | {after.Id.RawValue}"},
                        Timestamp = after.EditedAt ?? after.CreatedAt
                    };

                    await channel.ReplyAsync(embed);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for Message Updated - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(MessageDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var ch = e.Channel;
                var msg = e.Message;
                if (!(ch is CachedTextChannel chx)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(chx.Guild);
                    if (!cfg.LogMsg.HasValue) return;
                    var channel = chx.Guild.GetTextChannel(cfg.LogMsg.Value);
                    if (channel == null) return;
                    if (!msg.HasValue) return;
                    if (msg.Value.Author.IsBot) return;
                    var embed = new LocalEmbedBuilder
                    {
                        Description = msg.Value.Content.Truncate(1900),
                        Color = _colourService.Get(chx.Guild.Id.RawValue),
                        Author = new LocalEmbedAuthorBuilder {Name = "Message Deleted"},
                        Title = $"{msg.Value.Author} deleted a message in {chx.Name}",
                        Timestamp = msg.Value.CreatedAt,
                        Footer = new LocalEmbedFooterBuilder
                            {Text = $"User: {msg.Value.Author.Id.RawValue} | Message ID: {msg.Id.RawValue}"}
                    };

                    if (msg.Value.Attachments.Count > 0 && !chx.IsNsfw)
                    {
                        var file = msg.Value.Attachments.FirstOrDefault();
                        if (file != null)
                            embed.AddField(x =>
                            {
                                x.Name = "File";
                                x.IsInline = false;
                                x.Value = msg.Value.Attachments.FirstOrDefault()?.Url;
                            });
                    }

                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {chx.Guild.Id.RawValue} for Message Deleted - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessagesBulkDeleted(MessagesBulkDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var ch = e.Channel;
                var messages = e.Messages;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(ch.Guild);
                    if (!cfg.LogMsg.HasValue) return;
                    var logChannel = ch.Guild.GetTextChannel(cfg.LogMsg.Value);
                    if (logChannel == null) return;

                    var messageContent = new List<string>();
                    var content = new StringBuilder();
                    foreach (var x in messages)
                    {
                        if(!x.HasValue) continue;
                        var user = x.Value.Author;
                        if (content.Length + x.Value.Content.Length >= 1950)
                        {
                            messageContent.Add(content.ToString());
                            content.Clear();
                        }

                        content.AppendLine($"{user.Mention}: {x.Value.Content}");
                    }

                    if (content.Length > 0) messageContent.Add(content.ToString());

                    for (var i = 0; i < messageContent.Count; i++)
                    {
                        var embed = new LocalEmbedBuilder
                        {
                            Description = messageContent[i],
                            Color = _colourService.Get(ch.Guild.Id.RawValue),
                            Title = $"Bulk delete in {ch.Name}"
                        };
                        await logChannel.ReplyAsync(embed);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {ch.Guild.Id.RawValue} for Bulk Message Deleted - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}