using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Bot.Services.ImageGen;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly ImageGenerator _img;
        private readonly DbService _db;
        
        public WelcomeService(DiscordSocketClient client, DbService db, ImageGenerator img)
        {
            _client = client;
            _db = db;
            _img = img;

            _client.UserJoined += WelcomeUser;
            _client.LeftGuild += LeftGuild;
        }

        private Task WelcomeUser(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (user.IsBot) return;
                if (OnCooldown(user)) return;
                var cfg = await _db.GetOrCreateWelcomeConfigAsync(user.Guild);
                if (!cfg.Channel.HasValue) return;
                if (IsRatelimited(user, cfg)) return;
                var msg = CreateMessage(cfg.Message, user, user.Guild);
                IMessage message;
                if (cfg.Banner)
                {
                    var banner = await _img.WelcomeBuilder(user);
                    banner.Seek(0, SeekOrigin.Begin);
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
            });
            return Task.CompletedTask;
        }

        private Task LeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                var banners = _db.WelcomeBanners.Where(x => x.GuildId == guild.Id);
                _db.WelcomeBanners.RemoveRange(banners);
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }
    }
}