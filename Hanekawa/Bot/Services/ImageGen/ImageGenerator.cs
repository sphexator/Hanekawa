using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator : INService
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly DbService _db;
        private readonly ImageGenerator _image;
        private readonly ExpService _expService;

        private readonly GraphicsOptions _options = new GraphicsOptions(true);
        private readonly TextGraphicsOptions _centerText = new TextGraphicsOptions
        {
            Antialias = true,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        private readonly TextGraphicsOptions _rightText = new TextGraphicsOptions
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Antialias = true
        };
        private readonly TextGraphicsOptions _leftText = new TextGraphicsOptions
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Antialias = true

        };

        // Fonts
        private readonly FontCollection _fonts;
        private readonly FontFamily _times;
        private readonly FontFamily _arial;

        // Welcome
        private readonly Font _welcomeFontRegular;
        private readonly Image<Rgba32> _welcomeTemplate;

        // Profile
        private readonly Font _profileName;
        private readonly Font _profileText;
        private readonly Image<Rgba32> _profileTemplate;

        public ImageGenerator(HttpClient client, Random random, DbService db, ImageGenerator image,
            ExpService expService)
        {
            _client = client;
            _random = random;
            _db = db;
            _image = image;
            _expService = expService;

            _fonts = new FontCollection();
            _times = _fonts.Install("Data/Fonts/TIMES.TTF");
            _arial = _fonts.Install("Data/Fonts/ARIAL.TFF");

            _welcomeFontRegular = new Font(_times, 33, FontStyle.Regular);
            _welcomeTemplate = Image.Load("Data/Welcome/Default.png");

            _profileText = new Font(_arial, 20, FontStyle.Regular);
            _profileName = new Font(_arial, 32, FontStyle.Regular);
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