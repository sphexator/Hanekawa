﻿using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Humanizer;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        public async Task MuteAsync(IMember target, IMember staff, string reason, DbService db, TimeSpan? duration = null)
        {
            var guild = _bot.GetGuild(target.GuildId);
            var cfg = await db.GetOrCreateLoggingConfigAsync(target.GuildId.RawValue);
            if (!cfg.LogBan.HasValue) return;
            if(!guild.Channels.TryGetValue(cfg.LogBan.Value, out var channel)) return;
            var caseId = await db.CreateCaseId(target, guild, DateTime.UtcNow, ModAction.Mute);
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder { Name = $"Case ID: {caseId.Id} - User Muted | {target}" },
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = target.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                },
                Footer = new LocalEmbedFooterBuilder { Text = $"Username: {target} ({target.Id.RawValue})", IconUrl = target.GetAvatarUrl() }
            };
            if (duration.HasValue)
                embed.Fields.Add(new LocalEmbedFieldBuilder
                    {Name = "Duration", Value = $"{duration.Value.Humanize(2)}", IsInline = false});

            var msg = await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
            {
                Embed = embed,
                IsTextToSpeech = false,
                Mentions = LocalMentionsBuilder.None,
                Reference = null,
                Attachments = null,
                Content = null
            }.Build());
            caseId.MessageId = msg.Id.RawValue;
            await db.SaveChangesAsync();
        }

        public async Task WarnAsync(WarnReason warn, IMember target, IMember staff, string reason, DbService db, TimeSpan? duration = null)
        {
            var cfg = await db.GetOrCreateLoggingConfigAsync(target.GuildId);
            if (!cfg.LogWarn.HasValue) return;
            var guild = _bot.GetGuild(target.GuildId);
            if (!guild.Channels.TryGetValue(cfg.LogWarn.Value, out var channel)) return;

            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Timestamp = DateTimeOffset.UtcNow,
                Author = new LocalEmbedAuthorBuilder { Name = $"User {warn}" },
                Footer = new LocalEmbedFooterBuilder { Text = $"Username: {target} ({target.Id.RawValue})" },
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "User", Value = target.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Moderator", Value = staff.Mention, IsInline = false},
                    new LocalEmbedFieldBuilder {Name = "Reason", Value = reason, IsInline = false}
                }
            };
            if (duration.HasValue)
                embed.Fields.Add(new LocalEmbedFieldBuilder
                    { Name = "Duration", Value = $"{duration.Value.Humanize(2)}", IsInline = false });

            await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
            {
                Embed = embed,
                IsTextToSpeech = false,
                Mentions = LocalMentionsBuilder.None,
                Reference = null,
                Attachments = null,
                Content = null
            }.Build());
        }
    }
}