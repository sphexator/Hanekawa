using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core;
using Hanekawa.Database.Extensions;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        public async Task Mute(SocketGuildUser user, SocketGuildUser staff, string reason)
        {
            var cfg = await _db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await _db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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
            await _db.SaveChangesAsync();
        }
        
        public async Task Mute(SocketGuildUser user, SocketGuildUser staff, string reason, TimeSpan duration)
        {
            var cfg = await _db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await _db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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
            await _db.SaveChangesAsync();
        }
    }
}
