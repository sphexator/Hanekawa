using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.Extensions.Logging;

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

                        var embed = new EmbedBuilder().CreateDefault($"{user.Mention} updated a message in {chx.Mention}", user.Guild.Id);
                        embed.Author = new EmbedAuthorBuilder { Name = "Message Updated" };
                        embed.Timestamp = after.EditedTimestamp ?? after.Timestamp;
                        embed.Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder { Name = "Updated Message", Value = after.Content.Truncate(980), IsInline = true },
                            new EmbedFieldBuilder { Name = "Old Message", Value = beforeMsg.Content.Truncate(980), IsInline = true }
                        };
                        embed.Footer = new EmbedFooterBuilder { Text = $"User: {user.Id} | {after.Id}" };

                        await channel.ReplyAsync(embed);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"Error in Message Updated log in {user.Guild.Id} - {e.Message}");
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

                        if (!message.HasValue) await message.GetOrDownloadAsync();

                        var embed = new EmbedBuilder
                        {
                            Color = Color.Purple,
                            Author = new EmbedAuthorBuilder { Name = "Message Deleted" },
                            Description = $"{message.Value.Author.Mention} deleted a message in {chx.Mention}",
                            Timestamp = DateTimeOffset.UtcNow,
                            Footer = new EmbedFooterBuilder
                                { Text = $"User: {message.Value.Author.Id} | Message ID: {message.Value.Id}" },
                            Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                    {Name = "Message", Value = message.Value.Content.Truncate(900), IsInline = false}
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
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"Error in Message Deleted log in {chx.Guild.Id} - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}