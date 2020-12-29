using System;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Logging
{
    public partial class LogService
    {
        private Task UserLeft(MemberLeftEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.User;
                var guild = e.Guild;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                    if (!cfg.LogJoin.HasValue) return;
                    var channel = guild.GetTextChannel(cfg.LogJoin.Value);
                    if (channel == null) return;

                    var embed = new LocalEmbedBuilder
                    {
                        Description = $"📤 {user.Mention} has left ( *{user.Id.RawValue}* )",
                        Color = Color.Red,
                        Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user}"},
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    var gusr = guild.GetMember(user.Id);
                    if (gusr != null)
                    {
                        var roles = new StringBuilder();
                        foreach (var role in gusr.Roles) roles.Append($"{role.Value.Name}, ");
                        embed.AddField("Time in server", (DateTimeOffset.UtcNow - gusr.JoinedAt).Humanize());
                        embed.AddField("Roles", roles.ToString());
                    }

                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception exception)
                {
                    _log.LogAction(LogLevel.Error, exception,
                        $"(Log Service) Error in {guild.Id.RawValue} for Join Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task UserJoined(MemberJoinedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                    if (!cfg.LogJoin.HasValue) return;
                    var channel = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                    if (channel == null) return;

                    var embed = new LocalEmbedBuilder
                    {
                        Description = $"📥 {user.Mention} has joined ( *{user.Id.RawValue}* )\n" +
                                      $"Account created: {user.CreatedAt.Humanize()}",
                        Color = Color.Green,
                        Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user}"},
                        Timestamp = DateTimeOffset.UtcNow
                    };

                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception exception)
                {
                    _log.LogAction(LogLevel.Error, exception,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for Join Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}