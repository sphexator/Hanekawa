using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserLeft(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                        if (!cfg.LogJoin.HasValue) return;
                        var channel = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                        if (channel == null) return;

                        var embed = new EmbedBuilder
                        {
                            Description = $"📤 {user.Mention} has left ( *{user.Id}* )",
                            Color = Color.Green,
                            Footer = new EmbedFooterBuilder {Text = $"Username: {user}"},
                            Timestamp = DateTimeOffset.UtcNow
                        };

                        await channel.SendMessageAsync(null, false, embed.Build());
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id} for Join Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                        if (!cfg.LogJoin.HasValue) return;
                        var channel = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                        if (channel == null) return;

                        var embed = new EmbedBuilder
                        {
                            Description = $"📥 {user.Mention} has joined ( *{user.Id}* )\n" +
                                          $"Account created: {user.CreatedAt.Humanize()}",
                            Color = Color.Green,
                            Footer = new EmbedFooterBuilder {Text = $"Username: {user}"},
                            Timestamp = DateTimeOffset.UtcNow
                        };

                        await channel.SendMessageAsync(null, false, embed.Build());
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Log Service) Error in {user.Guild.Id} for Join Log - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}