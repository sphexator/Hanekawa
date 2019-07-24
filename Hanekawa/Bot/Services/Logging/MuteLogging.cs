using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        public async Task Mute(SocketGuildUser user, SocketGuildUser staff, string reason, DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
            var embed = new EmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new EmbedAuthorBuilder {Name = $"Case ID: {caseId} - User Muted | {user}"},
                Footer = new EmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"},
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            var msg = await channel.SendMessageAsync(null, false, embed.Build());
            caseId.MessageId = msg.Id;
            await db.SaveChangesAsync();
        }

        public async Task Mute(SocketGuildUser user, SocketGuildUser staff, string reason, TimeSpan duration,
            DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
            var embed = new EmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new EmbedAuthorBuilder {Name = $"Case ID: {caseId} - User Muted | {user}"},
                Footer = new EmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"},
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false},
                    new EmbedFieldBuilder {Name = "Duration", Value = $"{duration.Humanize()}", IsInline = false}
                }
            };
            var msg = await channel.SendMessageAsync(null, false, embed.Build());
            caseId.MessageId = msg.Id;
            await db.SaveChangesAsync();
        }
    }
}