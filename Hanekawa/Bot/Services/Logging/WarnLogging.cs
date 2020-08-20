using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        public async Task MuteWarn(CachedMember user, CachedMember staff, string reason, TimeSpan duration,
            DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogWarn.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogWarn.Value);
            if (channel == null) return;

            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthorBuilder {Name = "User Mute Warned"},
                Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user} ({user.Id.RawValue})"},
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Duration", Value = $"{duration.Humanize()}", IsInline = false}
                }
            };
            await channel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task Warn(CachedMember user, CachedMember staff, string reason, DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogWarn.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogWarn.Value);
            if (channel == null) return;

            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthorBuilder {Name = "User Warned"},
                Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user} ({user.Id.RawValue})"},
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            await channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}