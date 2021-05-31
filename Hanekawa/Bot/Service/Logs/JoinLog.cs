using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Disqord.Webhook;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Quartz.Util;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService : DiscordClientService
    {
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;
        private readonly IWebhookClientFactory _webhookClientFactory;
        private readonly Hanekawa _bot;
        private readonly CacheService _cache;

        public LogService(ILogger<LogService> logger, DiscordClientBase client, Hanekawa bot, IServiceProvider provider, 
            CacheService cache, IWebhookClientFactory webhookClientFactory) : base(logger, client)
        {
            _bot = bot;
            _provider = provider;
            _cache = cache;
            _webhookClientFactory = webhookClientFactory;
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            var guild = _bot.GetGuild(e.GuildId);
            if(guild == null) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
            if (!cfg.LogJoin.HasValue) return;
            if (guild.GetChannel(cfg.LogJoin.Value) is not ITextChannel channel) return;
            var embed = new LocalEmbedBuilder
            {
                Description = $"📥 {e.Member.Mention} has joined ( *{e.Member.Id}* )\n" +
                              $"Account created: {e.Member.CreatedAt.Humanize()}",
                Color = Color.Green,
                Footer = new LocalEmbedFooterBuilder {Text = $"Username: {e.Member}"},
                Timestamp = DateTimeOffset.UtcNow
            };
            try
            {
                var inviteeInfo = await GetInvite(guild);
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

                var builder = new LocalWebhookMessageBuilder
                {
                    Embeds = new List<LocalEmbedBuilder> {embed},
                    Mentions = LocalMentionsBuilder.None,
                    IsTextToSpeech = false,
                    Name = guild.GetCurrentUser().DisplayName(),
                    AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
                };
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookJoinId.Value, cfg.WebhookJoin);
                await webhook.ExecuteAsync(builder.Build());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Warn, exception, $"No valid webhook for member joined, re-creating");
                var builder = new LocalWebhookMessageBuilder 
                {
                    Embeds = new List<LocalEmbedBuilder> {embed},
                    Mentions = LocalMentionsBuilder.None,
                    IsTextToSpeech = false,
                    Name = guild.GetCurrentUser().DisplayName(),
                    AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
                };
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookJoin != webhook.Token) cfg.WebhookJoin = webhook.Token;
                if (!cfg.WebhookJoinId.HasValue || cfg.WebhookJoinId.Value != webhook.Id)
                    cfg.WebhookJoinId = webhook.Id;
                await webhook.ExecuteAsync(builder.Build());
                await db.SaveChangesAsync();
            }
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            var guild = e.User.GetGatewayClient().GetGuild(e.GuildId);
            if(guild == null) return;
            var user = e.User;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(guild);
            if (!cfg.LogJoin.HasValue) return;
            if (guild.GetChannel(cfg.LogJoin.Value) is not ITextChannel channel) return;
            var embed = new LocalEmbedBuilder
            {
                Description = $"📤 {user.Mention} has left ( *{user.Id}* )",
                Color = Color.Red,
                Footer = new LocalEmbedFooterBuilder {Text = $"Username: {user}"},
                Timestamp = DateTimeOffset.UtcNow
            };
            if (user is CachedMember gUser)
            {
                var roles = new StringBuilder();
                foreach (var role in gUser.GetRoles().Values) roles.Append($"{role.Name}, ");
                if (gUser.JoinedAt.HasValue)
                    embed.AddField("Time in server", (DateTimeOffset.UtcNow - gUser.JoinedAt.Value).Humanize());
                if (roles.Length > 0) embed.AddField("Roles", roles.ToString().Truncate(1000));
            }

            var builder = new LocalWebhookMessageBuilder
            {
                Embeds = new List<LocalEmbedBuilder> {embed},
                Mentions = LocalMentionsBuilder.None,
                IsTextToSpeech = false,
                Name = guild.GetCurrentUser().DisplayName(),
                AvatarUrl = guild.GetCurrentUser().GetAvatarUrl()
            };
            try
            {
                var webhook = _webhookClientFactory.CreateClient(cfg.WebhookJoinId.Value, cfg.WebhookJoin);
                await webhook.ExecuteAsync(builder.Build());
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, ex, $"No valid webhook for member left, re-creating");
                var webhook = await channel.GetOrCreateWebhookClientAsync();
                if (cfg.WebhookJoin != webhook.Token) cfg.WebhookJoin = webhook.Token;
                if (!cfg.WebhookJoinId.HasValue || cfg.WebhookJoinId.Value != webhook.Id)
                    cfg.WebhookJoinId = webhook.Id;
                await webhook.ExecuteAsync(builder.Build());
                await db.SaveChangesAsync();
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
                    tempInvites.TryAdd(x.Code, new Tuple<Snowflake?, int>(x.Inviter.Id, x.Metadata.Uses));

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