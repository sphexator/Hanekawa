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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly bool _disableBanner;
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

            _disableBanner = false;

            Directory.CreateDirectory("Data/Welcome/");
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

        private Task WelcomeToggler(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new hanekawaContext())
                {
                    await db
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
                if (!CheckCooldown(user as IGuildUser)) return;
                using (var db = new hanekawaContext())
                {
                    var cfg = await db.GuildConfigs.FindAsync(user.Guild.Id);
                    if (cfg.WelcomeToggle == false) return;
                    if (CheckCooldown(user) == false) return;
                    if (cfg.IsWelcomeImage)
                        await WelcomeBanner(user.Guild.GetTextChannel((ulong)cfg.WelcomeMessageChannelId), user)
                            .ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private static async Task WelcomeBanner(ISocketMessageChannel ch, IGuildUser user)
        {
            try
            {
                var stream = await ImageGeneratorAsync(user);
                stream.Seek(0, SeekOrigin.Begin);
                await ch.SendFileAsync(stream, "welcome.png");
                stream.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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

        private static async Task<MemoryStream> GetAvatarAsync(IUser user)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var response = await client.GetStreamAsync(user.GetAvatar());
                using (var img = Image.Load(response))
                {
                    img.Mutate(x => x.ConvertToAvatar(new Size(60, 60), 32));
                    img.Save(stream, new PngEncoder());
                    img.Dispose();
                }
            }

            return stream;
        }

        private static async Task<Stream> ImageGeneratorAsync(IGuildUser user)
        {
            var stream = new MemoryStream();
            var toLoad = GetImage(user.Guild);
            using (var img = Image.Load(toLoad, new PngDecoder()))
            {
                var avatar = await GetAvatarAsync(user);
                var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                var text = user.Username.Truncate(15);

                img.Mutate(ctx => ctx
                    .DrawImage(Image.Load(avatar.GetBuffer(), new PngDecoder()), new Size(60, 60), new Point(10, 10),
                        GraphicsOptions.Default)
                    .DrawText(text, font, Rgba32.White, new PointF(245, 51), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Antialias = true,
                        ApplyKerning = true
                    }));
                img.Save(stream, new PngEncoder());

                img.Dispose();
                avatar.Dispose();
            }

            return stream;
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