using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.WebUI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.WebUI.Bot.Service.Logs
{
    public partial class LogService
    {
        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            var guild = _bot.GetGuild(e.NewMember.GuildId);
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<LoggingConfig>(guild.Id);
            if (!cfg.LogAvi.HasValue) return;
            if (guild.GetChannel(cfg.LogAvi.Value) is not ITextChannel channel) return;

            var embed = new LocalEmbed
            {
                Footer = new LocalEmbedFooter
                    {Text = $"Username: {e.NewMember} ({e.NewMember.Id})", IconUrl = guild.GetIconUrl()}
            };
            if (e.OldMember.Nick != e.NewMember.Nick)
            {
                embed.Author = new LocalEmbedAuthor
                    {Name = "Nickname Change", IconUrl = e.NewMember.GetAvatarUrl()};
                embed.AddField("Old Nick", e.OldMember.Nick ?? e.NewMember.Name);
                embed.AddField("New Nick", e.NewMember.Nick ?? e.NewMember.Name);
            }

            if (e.OldMember.Name != e.NewMember.Name)
            {
                embed.Author = new LocalEmbedAuthor
                    {Name = "Name Change", IconUrl = e.NewMember.GetAvatarUrl()};
                embed.AddField("Old Name", e.OldMember.Nick ?? e.NewMember.Name);
                embed.AddField("New Name", e.NewMember.Nick ?? e.NewMember.Name);
            }

            if (e.OldMember.AvatarHash != e.NewMember.AvatarHash)
            {
                embed.Author = new LocalEmbedAuthor
                    {Name = "Avatar Change", IconUrl = e.NewMember.GetAvatarUrl()};
                embed.ThumbnailUrl = e.OldMember.GetAvatarUrl();
                embed.ImageUrl = e.NewMember.GetAvatarUrl();
            }

            if (embed.Author == null) return;
            var builder = new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed> {embed},
                AllowedMentions = LocalAllowedMentions.None,
                IsTextToSpeech = false,
                Name = guild.Name,
                AvatarUrl = guild.GetIconUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookAviId.Value, cfg.WebhookAvi);
                await webhook.ExecuteAsync(builder);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for user log, re-creating");
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookAvi != webhook.Token) cfg.WebhookAvi = webhook.Token;
                if (!cfg.WebhookAviId.HasValue || cfg.WebhookAviId.Value != webhook.Id)
                    cfg.WebhookAviId = webhook.Id;
                await webhook.ExecuteAsync(builder);
                await db.SaveChangesAsync();
            }
        }
    }
}
