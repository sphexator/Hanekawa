using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Services.Welcome
{
    public class ImageGenerator : IHanaService
    {
        private readonly HttpClient _httpClient;
        private readonly WelcomeMessage _message;
        private readonly Random _random;

        public ImageGenerator(HttpClient httpClient, Random random, WelcomeMessage message)
        {
            _httpClient = httpClient;
            _random = random;
            _message = message;
        }

        public async Task<Stream> TestBanner(IGuildUser user, string banner) =>
            await CreateBanner(user, await GetImageUrlAsync(banner));

        public async Task<Stream> Banner(IGuildUser user) => await CreateBanner(user, await GetImageAsync(user.Guild));

        public async Task<Tuple<Stream, string>> Banner(IGuildUser user, string message)
        {
            var stream = await CreateBanner(user, await GetImageAsync(user.Guild));
            var msg = _message.Message(message, user, user.Guild as SocketGuild);
            return new Tuple<Stream, string>(stream, msg);
        }

        private async Task<Stream> CreateBanner(IUser user, Image<Rgba32> banner)
        {
            var stream = new MemoryStream();
            using (var img = banner)
            {
                var avatar = await GetAvatarAsync(user);

                var fonts = new FontCollection();
                var times = fonts.Install(@"Data/Fonts/TIMES.TTF");
                var font = new Font(times, 33, FontStyle.Regular);
                var text = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.Username.Truncate(15)));
                //var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                var optionsCenter = new TextGraphicsOptions
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                img.Mutate(ctx => ctx
                    .DrawImage(avatar, new Point(10, 10), GraphicsOptions.Default)
                    .DrawText(optionsCenter, text, font, Rgba32.White, new Point(245, 46)));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private async Task<Image<Rgba32>> GetImageAsync(IGuild guild)
        {
            using (var db = new DbService())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == guild.Id).ToListAsync();
                if (list.Count == 0) return GetDefaultImage();
                var rand = _random.Next(list.Count);
                return await GetImageUrlAsync(list[rand].Url);
            }
        }

        private async Task<Image<Rgba32>> GetImageUrlAsync(string image)
        {
            using (var img = Image.Load(await _httpClient.GetStreamAsync(image)))
            {
                img.Mutate(x => x.Resize(600, 78));
                return img.Clone();
            }
        }

        private static Image<Rgba32> GetDefaultImage()
        {
            using (var img = Image.Load("Data/Welcome/Default.png"))
            {
                img.Mutate(x => x.Resize(600, 78));
                return img.Clone();
            }
        }

        private async Task<Image<Rgba32>> GetAvatarAsync(IUser user)
        {
            var response = await _httpClient.GetStreamAsync(user.GetAvatar());
            using (var img = Image.Load(response))
            {
                return img.CloneAndConvertToAvatarWithoutApply(
                    new Size(60, 60), 30);
            }
        }
    }
}