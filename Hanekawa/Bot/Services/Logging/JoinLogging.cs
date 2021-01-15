using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Util;

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
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                if (!cfg.LogJoin.HasValue) return;
                var channel = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                if (channel == null) return;
                try
                {
                    Tuple<IUser, string> inviteeInfo = null;
                    var restInvites = await user.Guild.GetInvitesAsync();
                    if (!_invites.TryGetValue(user.Guild.Id.RawValue, out var invites))
                    {
                        await UpdateInvites(user, restInvites);
                    }
                    else
                    {
                        Tuple<string, ulong, int> check = null;
                        for (var i = 0; i < restInvites.Count; i++)
                        {
                            if(check != null) continue;
                            var x = restInvites[i];
                            foreach (var y in invites)
                            {
                                if(check != null) continue;
                                if (y.Item3 + 1 == x.Metadata.Uses) check = new Tuple<string, ulong, int>(y.Item1, y.Item2, y.Item3);
                            }
                        }
                        if (check != null)
                        {
                            invites.Remove(check);
                            invites.Add(new Tuple<string, ulong, int>(check.Item1, check.Item2, check.Item3 + 1));
                            var invitee = await e.Client.GetOrFetchUserAsync(check.Item2);
                            if (invitee != null)
                            {
                                inviteeInfo = new Tuple<IUser, string>(invitee, $"discord.gg/{check.Item1}");
                            }
                            _invites.AddOrUpdate(user.Guild.Id.RawValue, new HashSet<Tuple<string, ulong, int>>(),
                                (id, set) => invites);
                        }
                    }

                    var embed = new LocalEmbedBuilder
                    {
                        Description = $"📥 {user.Mention} has joined ( *{user.Id.RawValue}* )\n" +
                                      $"Account created: {user.CreatedAt.Humanize()}",
                        Color = Color.Green,
                        Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user}"},
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    if (inviteeInfo != null &&
                        !inviteeInfo.Item2.IsNullOrWhiteSpace())
                    {
                        var msg = new StringBuilder();
                        
                        msg.AppendLine($"{inviteeInfo.Item2}");
                        msg.AppendLine(inviteeInfo.Item1 != null
                            ? $"By: {inviteeInfo.Item1}"
                            : "By: User couldn't be found");
                        if(!msg.ToString().IsNullOrWhiteSpace()) embed.AddField("Invite", msg.ToString().Truncate(1000));
                    }
                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception exception)
                {
                    try
                    {
                        var embed = new LocalEmbedBuilder
                        {
                            Description = $"📥 {user.Mention} has joined ( *{user.Id.RawValue}* )\n" +
                                          $"Account created: {user.CreatedAt.Humanize()}",
                            Color = Color.Green,
                            Footer = new LocalEmbedFooterBuilder { Text = $"Username: {user}" },
                            Timestamp = DateTimeOffset.UtcNow
                        };
                        await channel.SendMessageAsync(null, false, embed.Build());
                    }
                    catch (Exception e1)
                    {
                        _log.LogAction(LogLevel.Error, e1,
                            $"(Log Service) Error in {user.Guild.Id.RawValue} for Join Log - {e1.Message}");
                    }
                    _log.LogAction(LogLevel.Error, exception,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for Join Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async  Task UpdateInvites(CachedMember user, IReadOnlyList<RestInvite> restInvites = null)
        {
            if (restInvites == null) restInvites = await user.Guild.GetInvitesAsync();
            var invites = _invites.GetOrAdd(user.Guild.Id.RawValue, new HashSet<Tuple<string, ulong, int>>());
            for (var i = 0; i < restInvites.Count; i++)
            {
                var x = restInvites[i];
                invites.Add(new Tuple<string, ulong, int>(x.Code, x.Metadata.Inviter.Id.RawValue, x.Metadata.Uses));
            }

            _invites.AddOrUpdate(user.Guild.Id.RawValue, new HashSet<Tuple<string, ulong, int>>(),
                (id, set) => invites);
        }
    }
}