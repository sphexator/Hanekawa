using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly ExpService _exp;
        private readonly ImageGenerator _img;
        private readonly InteractiveService _interactive;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;

        public WelcomeService(DiscordSocketClient client, ImageGenerator img, InternalLogService log,
            InteractiveService interactive, ExpService exp, IServiceProvider provider)
        {
            _client = client;
            _img = img;
            _log = log;
            _interactive = interactive;
            _exp = exp;
            _provider = provider;

            _client.UserJoined += WelcomeUser;
            _client.LeftGuild += LeftGuild;
        }

        private Task WelcomeUser(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (user.IsBot) return;
                if (OnCooldown(user)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateWelcomeConfigAsync(user.Guild);
                        if (!cfg.Channel.HasValue) return;
                        if (IsRatelimited(user, cfg)) return;
                        var msg = CreateMessage(cfg.Message, user, user.Guild);
                        IMessage message;
                        SocketTextChannel channel;
                        if (cfg.Banner)
                        {
                            var banner = await _img.WelcomeBuilder(user, db);
                            banner.Position = 0;
                            channel = user.Guild.GetTextChannel(cfg.Channel.Value);
                            message = await channel.SendFileAsync(banner, "Welcome.png", msg);
                        }
                        else
                        {
                            channel = user.Guild.GetTextChannel(cfg.Channel.Value);
                            message = await channel.SendMessageAsync(msg);
                        }

                        var del = DeleteWelcomeAsync(message, cfg);
                        var exp = WelcomeRewardAsync(channel, cfg, db);
                        await Task.WhenAll(del, exp);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Welcome Service) Error in {user.Guild.Id} for User Joined - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task WelcomeRewardAsync(SocketTextChannel channel, WelcomeConfig cfg, DbService db)
        {
            if (!cfg.Reward.HasValue) return;
            var response = await _interactive.NextMessageAsync(_client, null, channel.Id, "welcome", cfg.TimeToDelete);
            if (response == null) return;
            if (!(response.Author is SocketGuildUser user)) return;
            var userData = await db.GetOrCreateUserData(user);
            await _exp.AddExpAsync(user, userData, cfg.Reward.Value, 0, db);
        }

        private async Task DeleteWelcomeAsync(IMessage msg, WelcomeConfig cfg)
        {
            if (!cfg.TimeToDelete.HasValue) return;
            await Task.Delay(cfg.TimeToDelete.Value);
            await msg.TryDeleteMessageAsync();
        }

        private Task LeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var banners = db.WelcomeBanners.Where(x => x.GuildId == guild.Id);
                        db.WelcomeBanners.RemoveRange(banners);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Welcome Service) Error in {guild.Id} for Bot Left Guild - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}