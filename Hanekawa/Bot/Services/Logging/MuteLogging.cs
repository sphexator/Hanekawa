using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        public async Task Mute(CachedMember user, CachedMember staff, string reason, DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthorBuilder {Name = $"Case ID: {caseId.Id} - User Muted | {user}"},
                Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"},
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            var msg = await channel.SendMessageAsync(null, false, embed.Build());
            caseId.MessageId = msg.Id;
            await db.SaveChangesAsync();
        }

        public async Task Mute(CachedMember user, CachedMember staff, string reason, TimeSpan duration,
            DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogBan.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
            if (channel == null) return;
            var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthorBuilder {Name = $"Case ID: {caseId.Id} - User Muted | {user}"},
                Footer = new LocalEmbedFooterBuilder() {Text = $"Username: {user} ({user.Id})"},
                Fields = 
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Duration", Value = $"{duration.Humanize(2)}", IsInline = false}
                }
            };
            var msg = await channel.SendMessageAsync(null, false, embed.Build());
            caseId.MessageId = msg.Id;
            await db.SaveChangesAsync();
        }
    }
}