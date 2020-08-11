using System;
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
                        banner.Seek(0, SeekOrigin.Begin);
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
            var response = await bot.GetInteractivity().WaitForMessageAsync(
                x => x.Message.Content == "welcome" && x.Message.Guild.Id.RawValue == cfg.GuildId && x.Message.Channel.Id.RawValue == channel.Id.RawValue,
                TimeSpan.FromMinutes(5));
            if (response == null) return;
            if (!(response.Message.Author is CachedMember user)) return;
            var userData = await db.GetOrCreateUserData(user);
            await _exp.AddExpAsync(user, userData, cfg.Reward.Value, 0, db);
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