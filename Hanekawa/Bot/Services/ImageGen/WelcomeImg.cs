using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

namespace Hanekawa.Bot.Services.ImageGen
{
    public class WelcomeImg : INService
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly DbService _db;
        private readonly ImageGenerator _image;
        
        private readonly GraphicsOptions _options = new GraphicsOptions(true);
        private readonly TextGraphicsOptions _center = new TextGraphicsOptions
        {
            Antialias = true,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        private readonly Font _regular;
        private readonly FontCollection _fonts;
        private readonly FontFamily _times;
        private readonly Image<Rgba32> _template;

        public WelcomeImg(HttpClient client, Random random, DbService db, ImageGenerator image)
        {
            _client = client;
            _random = random;
            _db = db;
            _image = image;
            
            _fonts = new FontCollection();
            _times = _fonts.Install("Data/Fonts/TIMES.TTF");
            _regular = new Font(_times, 33, FontStyle.Regular);
            _template = Image.Load("Data/Welcome/Default.png");
        }

        public async Task<Stream> Builder(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var img = await GetBanner(user.Guild.Id))
            {
                var avatar = await _image.GetAvatarAsync(user, new Size(60, 60), 32);
                
                img.Mutate(x => x.DrawImage(avatar, new Point(10, 10), _options));
                try
                {
                    img.Mutate(x => x.DrawText(_center, user.GetName().Truncate(15), _regular, Rgba32.White, new Point(245, 46)));
                }
                catch
                {
                    img.Mutate(x => x.DrawText(_center, "Bad Name", _regular, Rgba32.White, new Point(245, 46)));
                }

                img.Save(stream, new PngEncoder());
            }
            return stream;
        }

        private async Task<Image<Rgba32>> GetBanner(ulong guildId)
        {
            var list = await _db.WelcomeBanners.Where(x => x.GuildId == guildId).ToListAsync();
            if (list.Count == 0) return _template;
            using (var img = Image.Load(await _client.GetStreamAsync(list[_random.Next(list.Count)].Url)))
            {
                img.Mutate(x => x.Resize(600, 78));
                return img.Clone();
            }
        }
    }
}