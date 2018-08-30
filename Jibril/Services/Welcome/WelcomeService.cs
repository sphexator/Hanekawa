using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;

        // True = banners enabled
        // False = banners disabled 
        private ConcurrentDictionary<ulong, bool> DisableBanner { get; set; }
            = new ConcurrentDictionary<ulong, bool>();
        private ConcurrentDictionary<ulong, uint> JoinCount { get; set; }
            = new ConcurrentDictionary<ulong, uint>();

        private ConcurrentDictionary<ulong, bool> AntiRaidDisable { get; }
            = new ConcurrentDictionary<ulong, bool>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> WelcomeCooldown { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();

        public WelcomeService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _client = discord;
            _provider = provider;

            _client.UserJoined += Welcomer;
            _client.UserJoined += WelcomeToggler;
            _client.LeftGuild += BannerCleanup;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    DisableBanner.AddOrUpdate(x.GuildId, x.WelcomeChannel.HasValue, (arg1, b) => false);
                }
            }
        }

        public async Task TestBanner(ISocketMessageChannel ch, IGuildUser user, string backgroundUrl)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var response = await client.GetStreamAsync(backgroundUrl);
                var avatar = await GetAvatarAsync(user);
                using (var img = Image.Load(response))
                {
                    img.Mutate(x => x.Resize(600, 78));
                    var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                    var text = user.Username.Truncate(15);
                    var optionsCenter = new TextGraphicsOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    img.Mutate(ctx => ctx
                        .DrawImage(GraphicsOptions.Default, avatar, new Point(10, 10))
                        .DrawText(optionsCenter, text, font, Rgba32.White, new Point(245, 46)));
                    img.Save(stream, new PngEncoder());
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            await ch.SendFileAsync(stream, "testBanner.png");
        }

        private Task WelcomeToggler(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                uint counter;
                uint limit;
                using (var db = new DbService())
                {
                    var cfg = await db.GuildConfigs.FindAsync(user.Guild.Id);
                    limit = cfg.WelcomeLimit;
                    counter = JoinCount.GetOrAdd(user.Guild.Id, 0);
                }

                counter++;
                JoinCount.AddOrUpdate(user.Guild.Id, counter, (key, old) => old = counter);

                if (counter >= limit) AntiRaidDisable.AddOrUpdate(user.Guild.Id, true, (key, old) => old = true);
                await Task.Delay(5000);

                JoinCount.TryGetValue(user.Guild.Id, out var currentValue);
                if (currentValue <= limit) AntiRaidDisable.AddOrUpdate(user.Guild.Id, true, (key, old) => old = false);
                JoinCount.AddOrUpdate(user.Guild.Id, currentValue--, (key, old) => old = currentValue--);
            });
            return Task.CompletedTask;
        }

        private Task Welcomer(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (user.IsBot) return;
                    if (!CheckCooldown(user)) return;
                    var status = AntiRaidDisable.GetOrAdd(user.Guild.Id, false);
                    if (status) return;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                        if (!cfg.WelcomeChannel.HasValue) return;
                        await WelcomeBanner(user.Guild.GetTextChannel(cfg.WelcomeChannel.Value), user, cfg)
                            .ConfigureAwait(false);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private static async Task WelcomeBanner(ISocketMessageChannel ch, SocketGuildUser user, GuildConfig cfg)
        {
            var stream = await ImageGeneratorAsync(user);
            var msg = WelcomeMessage(cfg, user);
            stream.Seek(0, SeekOrigin.Begin);
            var welcMsg = await ch.SendFileAsync(stream, "welcome.png", msg);
            if (!cfg.WelcomeDelete.HasValue) return;
            await Task.Delay(cfg.WelcomeDelete.Value);
            try { await welcMsg.DeleteAsync(); }
            catch {/* IGNORE */}
        }

        private static async Task<Stream> ImageGeneratorAsync(IGuildUser user)
        {
            var stream = new MemoryStream();
            var toLoad = await GetImageAsync(user.Guild);
            var avatar = await GetAvatarAsync(user);
            using (var img = toLoad)
            {
                var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                var text = user.Username.Truncate(15);
                var optionsCenter = new TextGraphicsOptions
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                img.Mutate(ctx => ctx
                    .DrawImage(GraphicsOptions.Default, avatar, new Point(10, 10))
                    .DrawText(optionsCenter, text, font, Rgba32.White, new Point(245, 46)));
                img.Save(stream, new PngEncoder());
            }
            return stream;
        }

        private static async Task<Image<Rgba32>> GetImageAsync(IGuild guild)
        {
            using (var db = new DbService())
            using (var client = new HttpClient())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == guild.Id).ToListAsync();
                if (list.Count == 0) return GetDefaultImage();
                var rand = new Random().Next(list.Count);
                var response = await client.GetStreamAsync(list[rand].Url);
                using (var img = Image.Load(response))
                {
                    img.Mutate(x => x.Resize(600, 78));
                    return img.Clone();
                }
            }
        }

        private static Image<Rgba32> GetDefaultImage()
        {
            using (var img = Image.Load(@"Data\Welcome\Default.png"))
            {
                img.Mutate(x => x.Resize(600, 78));
                return img.Clone();
            }
        }

        private static async Task<Image<Rgba32>> GetAvatarAsync(IUser user)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStreamAsync(user.GetAvatar());
                using (var img = Image.Load(response))
                {
                    var avi = img.CloneAndConvertToAvatarWithoutApply(new Size(60, 60), 32);
                    return avi.Clone();
                }
            }
        }

        private static string WelcomeMessage(GuildConfig cfg, SocketGuildUser user)
        {
            if (cfg.WelcomeMessage.IsNullOrWhiteSpace()) return null;
            if (!UserRegex.IsMatch(cfg.WelcomeMessage)) return cfg.WelcomeMessage;
            var msg = UserRegex.Replace(cfg.WelcomeMessage, user.Mention);
            return msg;
        }

        private static Regex UserRegex => new Regex("%PLAYER%");

        private static Task BannerCleanup(SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                using (var db = new DbService())
                {
                    var banners = db.WelcomeBanners.Where(x => x.GuildId == guild.Id);
                    db.WelcomeBanners.RemoveRange(banners);
                    db.SaveChanges();
                }
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldown(IGuildUser usr)
        {
            var check = WelcomeCooldown.TryGetValue(usr.GuildId, out var cds);
            if (!check)
            {
                WelcomeCooldown.TryAdd(usr.GuildId, new ConcurrentDictionary<ulong, DateTime>());
                WelcomeCooldown.TryGetValue(usr.GuildId, out cds);
                cds.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            var userCheck = cds.TryGetValue(usr.Id, out var cd);
            if (!userCheck)
            {
                cds.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 600)) return false;
            cds.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }
    }
}