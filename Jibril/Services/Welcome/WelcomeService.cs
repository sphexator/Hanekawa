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
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> WelcomeCooldown { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();
        private ConcurrentDictionary<ulong, uint> JoinCount { get; set; }
            = new ConcurrentDictionary<ulong, uint>();

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
                using (var db = new DbService())
                {
                    await db.GuildConfigs.FindAsync(user.Guild.Id);
                    var counter = JoinCount.GetOrAdd(user.Guild.Id, 0);
                }
            });
            return Task.CompletedTask;
        }

        private Task Welcomer(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                if (user.IsBot) return;
                if (!CheckCooldown(user)) return;
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
                avatar.Seek(0, SeekOrigin.Begin);
                var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                var text = user.Username.Truncate(15);

                img.Mutate(ctx => ctx
                    .DrawImage(GraphicsOptions.Default, Image.Load(avatar, new PngDecoder()), new Point(10, 10))
                    .DrawText(text, font, Rgba32.White, new PointF(245, 51)));
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
            var rand = new Random();
            var randomImage = rand.Next(images.Count);
            var img = $"Data/Welcome/{guild.Id}/{images[randomImage].Name}";
            return img;
        }

        private static async Task<Stream> GetAvatarAsync(IUser user)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var response = await client.GetStreamAsync(user.GetAvatar());
                response.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(response))
                {
                    var avi = img.CloneAndConvertToAvatarWithoutApply(new Size(60, 60), 32);
                    avi.Save(stream, new PngEncoder());
                }
            }

            return stream;
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
            var checKey = WelcomeCooldown.ContainsKey(usr.GuildId);
            if (!checKey)
            {
                WelcomeCooldown.TryAdd(usr.Id, new ConcurrentDictionary<ulong, DateTime>());
                WelcomeCooldown.TryGetValue(usr.GuildId, out var userCollection);
                userCollection?.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            WelcomeCooldown.TryGetValue(usr.GuildId, out var collection);
            if (collection == null) return false;
            collection.TryGetValue(usr.Id, out var user);
            if (!((DateTime.UtcNow - user).TotalSeconds >= 600)) return false;
            collection.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => DateTime.UtcNow);
            return true;
        }
    }
}