using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.Extensions.Logging;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel ch)
        {
            _ = Task.Run(async () =>
            {
                if (!(after.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                if (!(ch is ITextChannel chx)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                        if (!cfg.LogMsg.HasValue) return;
                        var channel = user.Guild.GetTextChannel(cfg.LogMsg.Value);
                        if (channel == null) return;

                        IUserMessage beforeMsg;
                        if (!before.HasValue) beforeMsg = await before.GetOrDownloadAsync() as IUserMessage;
                        else beforeMsg = before.Value as IUserMessage;
                        if (beforeMsg == null) return;
                        if (beforeMsg.Content == after.Content) return;

                        var embed = new EmbedBuilder().Create(
                            $"{user.Mention} updated a message in {chx.Mention}", _colourService.Get(user.Guild.Id));
                        embed.Author = new EmbedAuthorBuilder {Name = "Message Updated"};
                        embed.Timestamp = after.EditedTimestamp ?? after.Timestamp;
                        embed.Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                                {Name = "Updated Message", Value = after.Content.Truncate(980), IsInline = true},
                            new EmbedFieldBuilder
                                {Name = "Old Message", Value = beforeMsg.Content.Truncate(980), IsInline = true}
                        };
                        embed.Footer = new EmbedFooterBuilder {Text = $"User: {user.Id} | {after.Id}"};

                        await channel.ReplyAsync(embed);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id} for Message Updated - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel ch)
        {
            _ = Task.Run(async () =>
            {
                if (!(ch is ITextChannel chx)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(chx.Guild);
                        if (!cfg.LogMsg.HasValue) return;
                        var channel = await chx.Guild.GetTextChannelAsync(cfg.LogMsg.Value);
                        if (channel == null) return;
                        var msg = await message.GetOrDownloadAsync();
                        if (msg == null) return;
                        if (msg.Author.IsBot) return;
                        var embed = new EmbedBuilder().Create(msg.Content.Truncate(1900), _colourService.Get(chx.GuildId));
                        embed.Author = new EmbedAuthorBuilder {Name = "Message Deleted"};
                        embed.Title = $"{msg.Author} deleted a message in {chx.Name}";
                        embed.Timestamp = msg.Timestamp;
                        embed.Footer = new EmbedFooterBuilder
                            {Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"};

                        if (msg.Attachments.Count > 0 && !chx.IsNsfw)
                        {
                            var file = msg.Attachments.FirstOrDefault();
                            if (file != null)
                                embed.AddField(x =>
                                {
                                    x.Name = "File";
                                    x.IsInline = false;
                                    x.Value = msg.Attachments.FirstOrDefault()?.Url;
                                });
                        }

                        await channel.SendMessageAsync(null, false, embed.Build());
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {chx.Guild.Id} for Message Deleted - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages,
            ISocketMessageChannel channel)
        {
            _ = Task.Run(async () =>
            {
                if (!(channel is ITextChannel ch)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(ch.Guild);
                        if (!cfg.LogMsg.HasValue) return;
                        var logChannel = await ch.Guild.GetTextChannelAsync(cfg.LogMsg.Value);
                        if (logChannel == null) return;

                        var messageContent = new List<string>();
                        var content = new StringBuilder();
                        foreach (var x in messages)
                        {
                            var msg = await x.GetOrDownloadAsync();
                            if(msg == null) continue;
                            var user = msg.Author;
                            if (content.Length + x.Value.Content.Length >= 1950)
                            {
                                messageContent.Add(content.ToString());
                                content.Clear();
                            }

                            content.AppendLine($"{user.Mention}: {msg.Content}");
                        }

                        if (content.Length > 0) messageContent.Add(content.ToString());

                        for (var i = 0; i < messageContent.Count; i++)
                        {
                            var embed = new EmbedBuilder().Create(messageContent[i], _colourService.Get(ch.GuildId));
                            embed.Title = $"Bulk delete in {ch.Name}";
                            await logChannel.ReplyAsync(embed);
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {ch.Guild.Id} for Bulk Message Deleted - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}