﻿using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUnbanned(MemberUnbannedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guild = e.Guild;
                var user = e.User;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                        if (!cfg.LogBan.HasValue) return;
                        var channel = guild.GetTextChannel(cfg.LogBan.Value);
                        if (channel == null) return;
                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Unban);
                        var embed = new LocalEmbedBuilder
                        {
                            Color = Color.Green,
                            Author = new LocalEmbedAuthorBuilder {Name = $"User Unbanned | Case ID: {caseId.Id} | {user}"},
                            Footer = new LocalEmbedFooterBuilder {Text = $"User ID: {user.Id}"},
                            Timestamp = DateTimeOffset.UtcNow,
                            Fields =
                            {
                                new LocalEmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = true},
                                new LocalEmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = true},
                                new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = true}
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

        private Task UserBanned(MemberBannedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.User;
                var guild = e.Guild;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                        if (!cfg.LogBan.HasValue) return;
                        var channel = guild.GetTextChannel(cfg.LogBan.Value);
                        if (channel == null) return;

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Ban);
                        var embed = new LocalEmbedBuilder
                        {
                            Color = Color.Red,
                            Author = new LocalEmbedAuthorBuilder {Name = $"User Banned | Case ID: {caseId.Id} | {user}"},
                            Fields =
                            {
                                new LocalEmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = false},
                                new LocalEmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = false},
                                new LocalEmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false}
                            },
                            Footer = new LocalEmbedFooterBuilder {Text = $"User ID: {user.Id}"},
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