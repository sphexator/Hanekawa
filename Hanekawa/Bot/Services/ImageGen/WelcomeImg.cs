using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Hanekawa.Bot.Services.ImageGen
{
    public class WelcomeImg
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly DbService _db;
        private readonly ImageGenerator _image;
        
        private readonly GraphicsOptions _options = new GraphicsOptions(true);
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
        }

        public async Task<Stream> Builder(IGuildUser user)
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(400, 400))
            {
                var avatar = await _image.GetAvatarAsync(user, new Size(60, 60), 32);
                
            }
            return stream;
        }
    }
}