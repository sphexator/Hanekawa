using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Jibril.Extensions;
using Jibril.Services.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace Jibril.Services.Welcome
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
            _client.JoinedGuild += CreateGuildDirectory;
            _client.LeftGuild += BannerCleanup;
            _client.UserJoined += WelcomeToggler;
            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    DisableBanner.AddOrUpdate(x.GuildId, x.WelcomeChannel.HasValue, (arg1, b) => false);
                }
            }

            Directory.CreateDirectory("Data/Welcome/");
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
                if (user.IsBot) return;
                if (!CheckCooldown(user)) return;
                var status = AntiRaidDisable.GetOrAdd(user.Guild.Id, false);
                if (status) return;
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (cfg.WelcomeChannel == null) return;
                    await WelcomeBanner(user.Guild.GetTextChannel(cfg.WelcomeChannel.Value), user)
                        .ConfigureAwait(false);
                }

            });

            return Task.CompletedTask;

        }

        private static async Task<Stream> ImageGeneratorAsync(IGuildUser user)
        {
            var stream = new MemoryStream();
            var toLoad = GetImage(user.Guild);
            using (var img = Image.Load(toLoad, new PngDecoder()))
            {
                var avatar = await GetAvatarAsync(user);
                //avatar.Seek(0, SeekOrigin.Begin);
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

        private static async Task WelcomeBanner(ISocketMessageChannel ch, IGuildUser user)
        {
            var stream = await ImageGeneratorAsync(user);
            stream.Seek(0, SeekOrigin.Begin);
            await ch.SendFileAsync(stream, "welcome.png");
        }

        private static string GetImage(IGuild guild)
        {
            var banner = new DirectoryInfo($"Data/Welcome/{guild.Id}/");
            var images = banner.GetFiles().ToList();
            if (images.Count == 0) return @"Data\Welcome\Default.png";
            var rand = new Random();
            var randomImage = rand.Next(images.Count);
            var img = $"Data/Welcome/{guild.Id}/{images[randomImage].Name}";
            return img;
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

        private static Task BannerCleanup(SocketGuild guild)
        {
            Directory.Delete($"Data/Welcome/{guild.Id}");
            return Task.CompletedTask;
        }

        private static Task CreateGuildDirectory(SocketGuild guild)
        {
            Directory.CreateDirectory($"Data/Welcome/{guild.Id}");
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