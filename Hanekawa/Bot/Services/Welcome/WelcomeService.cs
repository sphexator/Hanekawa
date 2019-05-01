using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly ImageGenerator _img;
        
        public WelcomeService(DiscordSocketClient client, ImageGenerator img)
        {
            _client = client;
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
            });
            return Task.CompletedTask;
        }

        private Task LeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var banners = db.WelcomeBanners.Where(x => x.GuildId == guild.Id);
                    db.WelcomeBanners.RemoveRange(banners);
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}