using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Humanizer;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        public async Task MuteWarn(SocketGuildUser user, SocketGuildUser staff, string reason, TimeSpan duration,
            DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogWarn.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogWarn.Value);
            if (channel == null) return;

            var embed = new EmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new EmbedAuthorBuilder {Name = "User Mute Warned"},
                Footer = new EmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"},
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false},
                    new EmbedFieldBuilder {Name = "Duration", Value = $"{duration.Humanize()}", IsInline = false}
                }
            };
            await channel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task Warn(SocketGuildUser user, SocketGuildUser staff, string reason, DbService db)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
            if (!cfg.LogWarn.HasValue) return;
            var channel = user.Guild.GetTextChannel(cfg.LogWarn.Value);
            if (channel == null) return;

            var embed = new EmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new EmbedAuthorBuilder {Name = "User Warned"},
                Footer = new EmbedFooterBuilder {Text = $"Username: {user} ({user.Id})"},
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "User", Value = user.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new EmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            await channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}