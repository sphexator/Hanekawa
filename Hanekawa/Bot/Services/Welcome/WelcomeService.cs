using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService : INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly ExpService _exp;
        private readonly ImageGenerator _img;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private static LocalCustomEmoji _expEmoji = null;

        public WelcomeService(Hanekawa client, ImageGenerator img, InternalLogService log, ExpService exp, IServiceProvider provider)
        {
            _client = client;
            _img = img;
            _log = log;
            _exp = exp;
            _provider = provider;

            _client.MemberJoined += WelcomeUser;
            _client.LeftGuild += LeftGuild;
        }

        private Task WelcomeUser(MemberJoinedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                if (user.IsBot) return;
                if (OnCooldown(user)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateWelcomeConfigAsync(user.Guild);
                    if (IsRatelimited(user, cfg)) return;
                    if (!cfg.Channel.HasValue) return;
                    var msg = CreateMessage(cfg.Message, user, user.Guild);
                    IMessage message;
                    var channel = user.Guild.GetTextChannel(cfg.Channel.Value);
                    if (channel == null) return;
                    if (cfg.Banner)
                    {
                        var banner = await _img.WelcomeBuilder(user, db);
                        banner.Position = 0;
                        message = await channel.SendMessageAsync(new LocalAttachment(banner, "Welcome.png"), msg);
                    }
                    else
                    {
                        if (msg == null) return;
                        message = await channel.SendMessageAsync(msg);
                    }

                    var del = DeleteWelcomeAsync(message, cfg);
                    var exp = WelcomeRewardAsync(_client, channel, cfg, db);
                    await Task.WhenAll(del, exp);
                    _log.LogAction(LogLevel.Information,$"(Welcome Service) User joined {user.Guild.Id.RawValue}");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Welcome Service) Error in {user.Guild.Id.RawValue} for User Joined - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task WelcomeRewardAsync(Hanekawa bot, CachedTextChannel channel, WelcomeConfig cfg, DbService db)
        {
            if (!cfg.Reward.HasValue) return;
            if (_expEmoji == null)
            {
                LocalCustomEmoji.TryParse("<:exp1:746344675585163384>", out var emote);
                _expEmoji = emote;
            }
            var users = new ConcurrentQueue<CachedMember>();
            var s = new Stopwatch();
            s.Start();
            while (s.Elapsed <= TimeSpan.FromMinutes(1))
            {
                var response = await bot.GetInteractivity().WaitForMessageAsync(
                    x => x.Message.Content.Contains("welcome", StringComparison.OrdinalIgnoreCase) &&
                         x.Message.Guild.Id.RawValue == cfg.GuildId &&
                         x.Message.Channel.Id.RawValue == channel.Id.RawValue &&
                        !x.Message.Author.IsBot,
                    TimeSpan.FromMinutes(1));
                _ = Task.Run(async () =>
                {
                    var res = response;
                    try
                    {
                        if (res == null) return;
                        if (!(res.Message.Author is CachedMember user)) return;
                        if (IsRewardCd(user)) return;
                        if (user.JoinedAt.AddHours(2) >= DateTimeOffset.UtcNow) return;
                        if (users.Contains(user)) return;
                        users.Enqueue(user);
                        if(!res.Message.Reactions.ContainsKey(_expEmoji)) 
                        { 
                            await res.Message.AddReactionAsync(_expEmoji);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e, e.Message);
                    }
                });
            }

            s.Stop();
            await Task.Delay(TimeSpan.FromSeconds(5));
            while (users.TryDequeue(out var user))
            {
                var userData = await db.GetOrCreateUserData(user);
                await _exp.AddExpAsync(user, userData, cfg.Reward.Value, 0, db);
            }
        }

        private async Task DeleteWelcomeAsync(IMessage msg, WelcomeConfig cfg)
        {
            if (!cfg.TimeToDelete.HasValue) return;
            await Task.Delay(cfg.TimeToDelete.Value);
            await msg.DeleteAsync();
        }

        private Task LeftGuild(LeftGuildEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guild = e.Guild;
                try
                {
                    using (var scope = _provider.CreateScope())
                    await using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
                    {
                        var banners = db.WelcomeBanners.Where(x => x.GuildId == guild.Id.RawValue);
                        db.WelcomeBanners.RemoveRange(banners);
                        await db.SaveChangesAsync();
                    }
                    _log.LogAction(LogLevel.Information, $"(Welcome Service) Cleaned up banners in {guild.Id.RawValue} as bot left server");
                }
                catch (Exception exception)
                {
                    _log.LogAction(LogLevel.Error, exception,
                        $"(Welcome Service) Error in {guild.Id.RawValue} for Bot Left Guild - {exception.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}