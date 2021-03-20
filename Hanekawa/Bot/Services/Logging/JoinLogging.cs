using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
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
                        embed.AddField("Roles", roles.ToString().Truncate(1000));
                    }

                    await channel.SendMessageAsync(null, false, embed.Build());
                }
                catch (Exception exception)
                {
                    _log.Log(NLog.LogLevel.Error, exception,
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
                    var inviteeInfo = await GetInvite(e);
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
                        _log.Log(NLog.LogLevel.Error, e1,
                            $"(Log Service) Error in {user.Guild.Id.RawValue} for Join Log - {e1.Message}");
                    }
                    _log.Log(NLog.LogLevel.Error, exception,
                        $"(Log Service) Error in {user.Guild.Id.RawValue} for Join Log - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task<Tuple<IUser, string>> GetInvite(MemberJoinedEventArgs e)
        {
            Tuple<IUser, string> inviteeInfo = null;
            var restInvites = await e.Member.Guild.GetInvitesAsync();
            if (!_invites.TryGetValue(e.Member.Guild.Id.RawValue, out var invites))
            {
                await UpdateInvites(e.Member, restInvites);
            }
            else
            {
                var tempInvites = new ConcurrentDictionary<string, Tuple<ulong, int>>();
                for (var i = 0; i < restInvites.Count; i++)
                {
                    var x = restInvites[i];
                    tempInvites.TryAdd(x.Code, new Tuple<ulong, int>(x.Metadata.Inviter.Id.RawValue, x.Metadata.Uses));
                }
                var change = invites.Except(tempInvites).ToList();
                var (code, tuple) = change.FirstOrDefault();
                if (code != null)
                {
                    var invitee = await e.Client.GetOrFetchUserAsync(tuple.Item1);
                    if (invitee != null)
                    {
                        inviteeInfo = new Tuple<IUser, string>(invitee, $"discord.gg/{code}");
                    }

                    await UpdateInvites(e.Member, restInvites);
                }
            }

            return inviteeInfo;
        }

        private async Task UpdateInvites(CachedMember user, IReadOnlyList<RestInvite> restInvites = null)
        {
            if (restInvites == null) restInvites = await user.Guild.GetInvitesAsync();
            var invites = new ConcurrentDictionary<string, Tuple<ulong, int>>();
            for (var i = 0; i < restInvites.Count; i++)
            {
                var x = restInvites[i];
                invites.TryAdd(x.Code, new Tuple<ulong, int>(x.Metadata.Inviter.Id.RawValue, x.Metadata.Uses));
            }

            _invites.AddOrUpdate(user.Guild.Id.RawValue, invites, (id, set) => invites);
        }
    }
}