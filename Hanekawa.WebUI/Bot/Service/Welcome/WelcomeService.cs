using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Account;
using Hanekawa.Entities.Config;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.WebUI.Extensions;
using Hanekawa.WebUI.Bot.Service.Experience;
using Hanekawa.WebUI.Bot.Service.ImageGeneration;
using Hanekawa.WebUI.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using LogLevel = NLog.LogLevel;

namespace Hanekawa.WebUI.Bot.Service.Welcome
{
    public class WelcomeService : DiscordClientService
    {
        private readonly Hanekawa _bot;
        private readonly ExpService _exp;
        private readonly ImageGenerationService _image;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;

        public WelcomeService(IServiceProvider provider, ExpService exp, ImageGenerationService image,
            ILogger<WelcomeService> logger, DiscordClientBase client) : base(logger, client)
        {
            _bot = (Hanekawa) client;
            _provider = provider;
            _exp = exp;
            _image = image;
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            var user = e.Member;
            if (user.IsBot) return;
            if (user.CreatedAt() >= DateTimeOffset.UtcNow.AddMinutes(10)) return;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateEntityAsync<WelcomeConfig>(user.GuildId);
                if (!cfg.Channel.HasValue) return;
                var guild = e.Member.GetGuild();
                var msg = MessageUtil.FormatMessage(cfg.Message, user, guild);
                IMessage message;
                var channel = guild.GetChannel(cfg.Channel.Value);
                if (channel == null) return;
                var textChannel = channel as ITextChannel;
                var client = await textChannel.GetOrCreateWebhookClientAsync();
                if (!cfg.WebhookId.HasValue || cfg.WebhookId.Value != client.Id)
                {
                    cfg.WebhookId = client.Id;
                    cfg.Webhook = client.Token;
                    await db.SaveChangesAsync();
                }

                if (cfg.Banner)
                {
                    var guildCfg = await db.GetOrCreateEntityAsync<GuildConfig>(guild.Id);
                    var (stream, isGif) = await _image.WelcomeAsync(user, db,
                        guildCfg.Premium.HasValue && guildCfg.Premium.Value >= DateTimeOffset.UtcNow);
                    stream.Position = 0;
                    message = isGif
                        ? await client.ExecuteAsync(new LocalWebhookMessage
                        {
                            Name = guild.Name,
                            AvatarUrl = guild.GetIconUrl(),
                            Attachments = new[] {new LocalAttachment(stream, "Welcome.gif")},
                            Content = msg,
                            Embeds = null,
                            AllowedMentions = LocalAllowedMentions.None,
                            IsTextToSpeech = false
                        })
                        : await client.ExecuteAsync(new LocalWebhookMessage
                        {
                            Name = guild.Name,
                            AvatarUrl = guild.GetIconUrl(),
                            Attachments = new[] {new LocalAttachment(stream, "Welcome.png")},
                            Content = msg,
                            Embeds = null,
                            AllowedMentions = LocalAllowedMentions.None,
                            IsTextToSpeech = false
                        });
                }
                else
                {
                    if (msg == null) return;
                    message = await client.ExecuteAsync(new LocalWebhookMessage
                    {
                        Name = guild.Name,
                        AvatarUrl = guild.GetIconUrl(),
                        Content = msg,
                        Attachments = null,
                        Embeds = null,
                        AllowedMentions = LocalAllowedMentions.None,
                        IsTextToSpeech = false
                    });
                }

                var source = new CancellationTokenSource();
                var del = DeleteBannerAsync(message, cfg);
                var exp = RewardAsync(channel, cfg, db, source.Token);
                await Task.WhenAny(del, exp);
                source.Cancel();
                _logger.Log(LogLevel.Info, $"(Welcome Service) User joined {e.GuildId}");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Welcome Service) Error in {e.GuildId} for User Joined - {exception.Message}");
            }
        }

        protected override async ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            try
            {
                using (var scope = _provider.CreateScope())
                await using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
                {
                    var banners = await db.WelcomeBanners.Where(x => x.GuildId == e.GuildId).ToArrayAsync();
                    db.WelcomeBanners.RemoveRange(banners);
                    await db.SaveChangesAsync();
                }

                _logger.Log(LogLevel.Info,
                    $"(Welcome Service) Cleaned up banners in {e.Guild.Id} as bot left server");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Welcome Service) Error in {e.GuildId} for Bot Left Guild - {exception.Message}");
            }
        }

        private async Task RewardAsync(ISnowflakeEntity channel, WelcomeConfig cfg, DbService db,
            CancellationToken token)
        {
            if (!cfg.Reward.HasValue) return;
            try
            {
                var users = new HashSet<IMember>();
                var s = new Stopwatch();
                s.Start();
                while (s.Elapsed <= TimeSpan.FromMinutes(1))
                {
                    var response = await _bot.GetInteractivity().WaitForMessageAsync(channel.Id,
                        x => x.Message.Content.Contains("welcome", StringComparison.OrdinalIgnoreCase),
                        TimeSpan.FromMinutes(1), token);
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            if (response?.Message.Author is not CachedMember user) return;
                            if (user.JoinedAt.Value.AddHours(2) >= DateTimeOffset.UtcNow) return;
                            if (users.Contains(user)) return;
                            users.Add(user);
                        }
                        catch (Exception e)
                        {
                            _logger.Log(LogLevel.Error, e, e.Message);
                        }
                    }, token);
                }

                s.Stop();
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                foreach (var x in users)
                {
                    var userData = await db.GetOrCreateEntityAsync<Account>(x.GuildId, x.Id);
                    await _exp.AddExpAsync(x, userData, cfg.Reward.Value, 0, db, ExpSource.Other);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, e.Message);
            }
        }

        private async Task DeleteBannerAsync(IMessage msg, WelcomeConfig cfg)
        {
            try
            {
                if (!cfg.TimeToDelete.HasValue) return;
                await Task.Delay(cfg.TimeToDelete.Value);
                await msg.DeleteAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Welcome Service) Couldn't delete banner in {cfg.GuildId}");
            }
        }
    }
}