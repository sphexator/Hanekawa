using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.ImageGeneration;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Hanekawa.Utility;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Welcome
{
    public class WelcomeService : INService
    {
        private readonly Hanekawa _bot;
        private readonly ExpService _exp;
        private readonly ImageGenerationService _image;
        private readonly Logger _logger;
        private readonly IServiceProvider _provider;

        public WelcomeService(Hanekawa bot, IServiceProvider provider, ExpService exp, ImageGenerationService image)
        {
            _bot = bot;
            _provider = provider;
            _exp = exp;
            _image = image;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task MemberJoinedAsync(MemberJoinedEventArgs e)
        {
            var user = e.Member;
            if (user.IsBot) return;
            if (user.CreatedAt >= DateTimeOffset.UtcNow.AddMinutes(10)) return;
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateWelcomeConfigAsync(user.GuildId);
                if (!cfg.Channel.HasValue) return;
                var guild = e.Member.GetGuild();
                var msg = MessageUtil.FormatMessage(cfg.Message, user, guild);
                IMessage message;
                var channel = guild.GetChannel(cfg.Channel.Value);
                if (channel == null) return;
                var textChannel = channel as ITextChannel;
                if (cfg.Banner)
                {
                    var guildCfg = await db.GetOrCreateGuildConfigAsync(guild);
                    var (stream, isGif) = await _image.WelcomeBuilderAsync(user, db,
                        guildCfg.Premium.HasValue && guildCfg.Premium.Value >= DateTimeOffset.UtcNow);
                    stream.Position = 0;
                    message = isGif
                        ? await textChannel.SendMessageAsync(new LocalMessageBuilder
                        {
                            Attachments = new List<LocalAttachment> {new(stream, "Welcome.gif")},
                            Content = msg,
                            Embed = null,
                            Mentions = LocalMentionsBuilder.None,
                            Reference = null,
                            IsTextToSpeech = false
                        }.Build())
                        : await textChannel.SendMessageAsync(new LocalMessageBuilder
                        {
                            Attachments = new List<LocalAttachment> {new(stream, "Welcome.png")},
                            Content = msg,
                            Embed = null,
                            Mentions = LocalMentionsBuilder.None,
                            Reference = null,
                            IsTextToSpeech = false
                        }.Build());
                }
                else
                {
                    if (msg == null) return;
                    message = await textChannel.SendMessageAsync(new LocalMessageBuilder
                    {
                        Content = msg,
                        Attachments = null,
                        Embed = null,
                        Mentions = LocalMentionsBuilder.None,
                        Reference = null,
                        IsTextToSpeech = false
                    }.Build());
                }

                var source = new CancellationTokenSource();
                var del = DeleteBannerAsync(message, cfg);
                var exp = RewardAsync(channel, cfg, db, source.Token);
                await Task.WhenAny(del, exp);
                source.Cancel();
                _logger.Log(LogLevel.Info, $"(Welcome Service) User joined {e.GuildId.RawValue}");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Welcome Service) Error in {e.GuildId.RawValue} for User Joined - {exception.Message}");
            }
        }

        public async Task LeftGuildAsync(LeftGuildEventArgs e)
        {
            try
            {
                using (var scope = _provider.CreateScope())
                await using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
                {
                    var banners = db.WelcomeBanners.Where(x => x.GuildId == e.GuildId);
                    db.WelcomeBanners.RemoveRange(banners);
                    await db.SaveChangesAsync();
                }

                _logger.Log(LogLevel.Info,
                    $"(Welcome Service) Cleaned up banners in {e.Guild.Id.RawValue} as bot left server");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Welcome Service) Error in {e.GuildId.RawValue} for Bot Left Guild - {exception.Message}");
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
                    var userData = await db.GetOrCreateUserData(x);
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