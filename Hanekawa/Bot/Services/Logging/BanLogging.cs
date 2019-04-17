using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core;
using Hanekawa.Database.Extensions;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserUnbanned(SocketUser user, SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                var cfg = await _db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogBan.HasValue) return;
                var channel = guild.GetTextChannel(cfg.LogBan.Value);
                if (channel == null) return;
                var caseId = await _db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Unban);
                var embed = new EmbedBuilder
                {
                    Color = Color.Green,
                    Author = new EmbedAuthorBuilder { Name = $"Case ID: {caseId} | {user}"},
                    Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}"},
                    Timestamp = DateTimeOffset.UtcNow,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = false},
                        new EmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = false},
                        new EmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false }
                    }
                };
                var msg = await channel.SendMessageAsync(null, false, embed.Build());
                caseId.MessageId = msg.Id;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task UserBanned(SocketUser user, SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                var cfg = await _db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogBan.HasValue) return;
                var channel = guild.GetTextChannel(cfg.LogBan.Value);
                if (channel == null) return;

                var caseId = await _db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Ban);
                var embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Author = new EmbedAuthorBuilder { Name = $"Case ID: {caseId} | {user}" },
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder {Name = "User", Value = $"{user.Mention}", IsInline = false },
                        new EmbedFieldBuilder {Name = "Moderator", Value = "N/A", IsInline = false },
                        new EmbedFieldBuilder {Name = "Reason", Value = "N/A", IsInline = false }
                    },
                    Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                    Timestamp = DateTimeOffset.UtcNow
                };
                var msg = await channel.SendMessageAsync(null, false, embed.Build());
                caseId.MessageId = msg.Id;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }
    }
}