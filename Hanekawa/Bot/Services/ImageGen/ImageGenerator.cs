using System;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator : INService
    {
        private readonly HttpClient _client;
        private readonly ExpService _expService;

        // Fonts
        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly TextGraphicsOptions _leftText = new TextGraphicsOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true
            },
            TextOptions = new TextOptions
            {
                HorizontalAlignment = HorizontalAlignment.Left
            }
        };
        private readonly TextGraphicsOptions _centerText = new TextGraphicsOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true
            },
            TextOptions = new TextOptions
            {
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        private readonly GraphicsOptions _options = new GraphicsOptions
        {
            Antialias = true
        };

        // Profile
        private readonly Font _profileName;
        private readonly Image _profileTemplate;
        private readonly Font _profileText;
        private readonly Random _random;

        private readonly TextGraphicsOptions _rightText = new TextGraphicsOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true
            },
            TextOptions = new TextOptions
            {
                HorizontalAlignment = HorizontalAlignment.Right
            }
        };
        private readonly FontFamily _times;

        // Welcome
        private readonly Font _welcomeFontRegular;
        private readonly Image _welcomeTemplate;

        // Hunger Games
        private readonly Font _hgTimes;

        public ImageGenerator(HttpClient client, Random random, ExpService expService)
        {
            _client = client;
            _random = random;
            _expService = expService;

            _fonts = new FontCollection();
            _times = _fonts.Install("Data/Fonts/TIMES.TTF");
            _arial = _fonts.Install("Data/Fonts/ARIAL.TTF");

            _welcomeFontRegular = new Font(_times, 33, FontStyle.Regular);
            _welcomeTemplate = Image.Load("Data/Welcome/Default.png", new PngDecoder {IgnoreMetadata = true });

            _profileText = new Font(_arial, 20, FontStyle.Regular);
            _profileName = new Font(_arial, 32, FontStyle.Regular);
            _profileTemplate = Image.Load("Data/Profile/Template/Template.png", new PngDecoder { IgnoreMetadata = true });

            _hgTimes = new Font(_times, 15, FontStyle.Regular);
        }

        private async Task<Image> GetAvatarAsync(CachedMember user, Size size, int radius)
        {
            var avatar = await _client.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png));
            var response = avatar.ToEditable();
            response.Position = 0;
            using var img = await Image.LoadAsync(response, new PngDecoder());
            return img.Clone(x => x.ConvertToAvatar(size, radius));
        }

        private async Task<Image> GetAvatarAsync(CachedMember user, Size size)
        {
            var avatar = await _client.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png));
            var response = avatar.ToEditable();
            response.Position = 0;
            using var img = await Image.LoadAsync(response);
            return img.Clone(x => x.Resize(size));
        }

        private async Task<Image> GetAvatarAsync(string imgUrl, Size size)
        {
            var avatar = await _client.GetStreamAsync(imgUrl);
            var response = avatar.ToEditable();
            response.Position = 0;
            using var img = await Image.LoadAsync(response, new PngDecoder());
            return img.Clone(x => x.Resize(size));
        }

        private async Task<Image> GetAvatarAsync(string imgUrl, Size size, int radius)
        {
            var avatar = await _client.GetStreamAsync(imgUrl);
            var response = avatar.ToEditable();
            response.Position = 0;
            using var img = await Image.LoadAsync(response, new PngDecoder());
            return img.Clone(x => x.ConvertToAvatar(size, radius));
        }
    }
}