using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
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

        public ImageGenerator(HttpClient client, Random random, DbService db, ImageGenerator image)
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

        private async Task<Image<Rgba32>> GetAvatarAsync(IUser user, Size size, int radius)
        {
            var response = await _client.GetStreamAsync(user.GetAvatarUrl());
            using (var img = Image.Load(response))
            {
                var avi = img.CloneAndConvertToAvatarWithoutApply(size, radius);
                return avi.Clone();
            }
        }

        private async Task<Image<Rgba32>> GetAvatarAsync(IUser user, Size size)
        {
            var response = await _client.GetStreamAsync(user.GetAvatarUrl());
            using (var img = Image.Load(response))
            {
                img.Mutate(x => x.Resize(size));
                return img.Clone();
            }
        }
    }
}