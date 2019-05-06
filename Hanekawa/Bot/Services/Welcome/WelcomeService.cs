using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly ImageGenerator _img;
        private readonly InternalLogService _log;
        
        public WelcomeService(DiscordSocketClient client, ImageGenerator img, InternalLogService log)
        {
            _client = client;
            _img = img;
            _log = log;

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
                        if (cfg.Banner)
                        {
                            var banner = await _img.WelcomeBuilder(user, db);
                            banner.Position = 0;
                            message = await user.Guild.GetTextChannel(cfg.Channel.Value)
                                .SendFileAsync(banner, "Welcome.png", msg);
                        }
                        else
                        {
                            message = await user.Guild.GetTextChannel(cfg.Channel.Value).SendMessageAsync(msg);
                        }
                        if (cfg.TimeToDelete.HasValue)
                        {
                            await Task.Delay(cfg.TimeToDelete.Value);
                            await message.DeleteAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Welcome Service) Error in {user.Guild.Id} for User Joined - {e.Message}");
                }
            });
            return Task.CompletedTask;
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
                    _log.LogAction(LogLevel.Error, e, $"(Welcome Service) Error in {guild.Id} for Bot Left Guild - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}