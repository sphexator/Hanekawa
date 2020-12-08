using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task ReactionAddLog(Disqord.Events.ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Channel is CachedTextChannel channel)) return;
                if (!e.User.HasValue) await e.User.FetchAsync();
                if (e.User.Value.IsBot) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var log = await db.GetOrCreateLoggingConfigAsync(channel.Guild);
                if (!log.LogReaction.HasValue) return;
                if (log.ReactionWebhook.IsNullOrWhiteSpace()) return;
                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder
                        {IconUrl = e.User.Value.GetAvatarUrl(), Name = e.User.Value.Name},
                    Color = _colourService.Get(channel.Guild.Id.RawValue),
                    Description = $"{e.User.Value.Name} added {e.Emoji} to a message in {channel.Mention}",
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                        {Text = $"Channel: {channel.Id.RawValue} - Message: {e.Message.Id.RawValue}"}
                }.Build();
                try
                {
                    var embeds = new List<LocalEmbed> { embed };
                    await RestWebhookClient.FromUrl(log.ReactionWebhook).ExecuteAsync(null, false, embeds, _client.CurrentUser.Name, _client.CurrentUser.GetAvatarUrl(), true);
                }
                catch
                {
                    var logChannel = channel.Guild.GetTextChannel(log.LogReaction.Value);
                    if (logChannel == null)
                    {
                        log.ReactionWebhook = null;
                        log.LogReaction = null;
                        await db.SaveChangesAsync();
                        return;
                    }
                    var webhooks = await channel.GetWebhooksAsync();
                    var check = webhooks.FirstOrDefault(x => x.Owner.Id == channel.Guild.CurrentMember.Id);
                    if (check == null)
                    {
                        log.ReactionWebhook = null;
                        log.LogReaction = null;
                        await db.SaveChangesAsync();
                        return;
                    }

                    await logChannel.SendMessageAsync(null, false, embed);
                }
            });
            return Task.CompletedTask;
        }

        private Task ReactionRemovedLog(Disqord.Events.ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Channel is CachedTextChannel channel)) return;
                if (!e.User.HasValue) await e.User.FetchAsync();
                if (e.User.Value.IsBot) return;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var log = await db.GetOrCreateLoggingConfigAsync(channel.Guild);
                if (!log.LogReaction.HasValue) return;
                if (log.ReactionWebhook.IsNullOrWhiteSpace()) return;
                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder
                    { IconUrl = e.User.Value.GetAvatarUrl(), Name = e.User.Value.Name },
                    Color = _colourService.Get(channel.Guild.Id.RawValue),
                    Description = $"{e.User.Value.Name} removed {e.Emoji} from a message in {channel.Mention}",
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                    { Text = $"Channel: {channel.Id.RawValue} - Message: {e.Message.Id.RawValue}" }
                }.Build();
                try
                {
                    var embeds = new List<LocalEmbed> { embed };
                    await RestWebhookClient.FromUrl(log.ReactionWebhook).ExecuteAsync(null, false, embeds, _client.CurrentUser.Name, _client.CurrentUser.GetAvatarUrl(), true);
                }
                catch
                {
                    var logChannel = channel.Guild.GetTextChannel(log.LogReaction.Value);
                    if (logChannel == null)
                    {
                        log.ReactionWebhook = null;
                        log.LogReaction = null;
                        await db.SaveChangesAsync();
                        return;
                    }
                    var webhooks = await channel.GetWebhooksAsync();
                    var check = webhooks.FirstOrDefault(x => x.Owner.Id == channel.Guild.CurrentMember.Id);
                    if (check == null)
                    {
                        log.ReactionWebhook = null;
                        log.LogReaction = null;
                        await db.SaveChangesAsync();
                        return;
                    }

                    await logChannel.SendMessageAsync(null, false, embed);
                }
            });
            return Task.CompletedTask;
        }
    }
}
