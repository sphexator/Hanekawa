using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUnbanned(SocketUser user, SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                        if (!cfg.LogBan.HasValue) return;
                        var channel = guild.GetTextChannel(cfg.LogBan.Value);
                        if (channel == null) return;
                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Unban);
                        var embed = new EmbedBuilder
                        {
                            Color = Color.Green,
                            Author = new EmbedAuthorBuilder { Name = $"Case ID: {caseId} | {user}" },
                            Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                            Timestamp = DateTimeOffset.UtcNow,
                            Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = false},
                                new EmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = false},
                                new EmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false}
                            }
                        };
                        var msg = await channel.SendMessageAsync(null, false, embed.Build());
                        caseId.MessageId = msg.Id;
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Log Service) Error in {guild.Id} for UnBan Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task UserBanned(SocketUser user, SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                        if (!cfg.LogBan.HasValue) return;
                        var channel = guild.GetTextChannel(cfg.LogBan.Value);
                        if (channel == null) return;

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Ban);
                        var embed = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Author = new EmbedAuthorBuilder { Name = $"Case ID: {caseId} | {user}" },
                            Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = false},
                                new EmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = false},
                                new EmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false}
                            },
                            Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                            Timestamp = DateTimeOffset.UtcNow
                        };
                        var msg = await channel.SendMessageAsync(null, false, embed.Build());
                        caseId.MessageId = msg.Id;
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Log Service) Error in {guild.Id} for Ban Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}