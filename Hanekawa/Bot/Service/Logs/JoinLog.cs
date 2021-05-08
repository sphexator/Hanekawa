using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz.Util;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService : INService
    {
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;
        
        public LogService(Hanekawa bot, IServiceProvider provider, CacheService cache)
        {
            _bot = bot;
            _provider = provider;
            _cache = cache;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task JoinLogAsync(MemberJoinedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if(guild == null) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
            if (!cfg.LogJoin.HasValue) return;
            var channel = guild.GetChannel(cfg.LogJoin.Value);
            if (channel == null) return;
            try
            {
                var inviteeInfo = await GetInvite(guild);
                var embed = new LocalEmbedBuilder
                {
                    Description = $"📥 {e.Member.Mention} has joined ( *{e.Member.Id.RawValue}* )\n" +
                                  $"Account created: {e.Member.CreatedAt.Humanize()}",
                    Color = Color.Green,
                    Footer = new LocalEmbedFooterBuilder { Text = $"Username: {e.Member}" },
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
                    if (!msg.ToString().IsNullOrWhiteSpace()) embed.AddField("Invite", msg.ToString().Truncate(1000));
                }

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
            catch (Exception exception)
            {
                try
                {
                    var embed = new LocalEmbedBuilder
                    {
                        Description = $"📥 {e.Member.Mention} has joined ( *{e.Member.Id.RawValue}* )\n" +
                                      $"Account created: {e.Member.CreatedAt.Humanize()}",
                        Color = Color.Green,
                        Footer = new LocalEmbedFooterBuilder { Text = $"Username: {e.Member}" },
                        Timestamp = DateTimeOffset.UtcNow
                    };
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
                catch (Exception e1)
                {
                    _logger.Log(LogLevel.Error, e1,
                        $"(Log Service) Error in {guild.Id.RawValue} for Join Log - {e1.Message}");
                }
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {guild.Id.RawValue} for Join Log - {exception.Message}");
            }
        }

        public async Task LeaveLogAsync(MemberLeftEventArgs e)
        {
            var guild = e.User.GetGatewayClient().GetGuild(e.GuildId);
            if(guild == null) return;
            var user = e.User;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
                if (!cfg.LogJoin.HasValue) return;
                var channel = guild.GetChannel(cfg.LogJoin.Value);
                if (channel == null) return;
                var embed = new LocalEmbedBuilder
                {
                    Description = $"📤 {user.Mention} has left ( *{user.Id.RawValue}* )",
                    Color = Color.Red,
                    Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user}"},
                    Timestamp = DateTimeOffset.UtcNow
                };
                if (user is CachedMember gUser)
                {
                    var roles = new StringBuilder();
                    foreach (var role in gUser.GetRoles().Values) roles.Append($"{role.Name}, ");
                    if(gUser.JoinedAt.HasValue) embed.AddField("Time in server", (DateTimeOffset.UtcNow - gUser.JoinedAt.Value).Humanize());
                    if(roles.Length > 0) embed.AddField("Roles", roles.ToString().Truncate(1000));
                }

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
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"(Log Service) Error in {guild.Id.RawValue} for Join Log - {exception.Message}");
            }
        }

        private async ValueTask<Tuple<IUser, string>> GetInvite(IGuild guild)
        {
            Tuple<IUser, string> inviteeInfo = null;
            var restInvites = await guild.FetchInvitesAsync();
            if (!_cache.TryGetInvite(guild.Id, out var invites))  _cache.UpdateInvites(guild.Id, restInvites);
            else
            {
                var tempInvites = new ConcurrentDictionary<string, Tuple<Snowflake?, int>>();
                foreach (var x in restInvites) 
                    tempInvites.TryAdd(x.Code, new Tuple<Snowflake?, int>(x.Inviter.Value.Id, x.Metadata.Uses));

                var change = invites.Except(tempInvites).ToList();
                var (code, (snowflake, _)) = change.FirstOrDefault();
                if (code == null) return null;
                if (!snowflake.HasValue) return null;
                var invitee = _bot.GetUser(new Snowflake(snowflake.Value));
                if (invitee != null)
                {
                    inviteeInfo = new Tuple<IUser, string>(invitee, $"discord.gg/{code}");
                }

                _cache.UpdateInvites(guild.Id, restInvites);
            }

            return inviteeInfo;
        }
    }
}
