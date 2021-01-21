using Disqord;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Quartz.Util;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Database.Tables.Config;

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
        private readonly Image _welcomeTemplate;
        private readonly WelcomeBanner _defWelcomeBanner;

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
            
            _welcomeTemplate = Image.Load("Data/Welcome/Default.png", new PngDecoder {IgnoreMetadata = true });
            _defWelcomeBanner = new WelcomeBanner
            {
                AvatarSize = 60,
                AviPlaceX = 10,
                AviPlaceY = 10,
                TextSize = 33,
                TextPlaceX = 245,
                TextPlaceY = 40,
                IsNsfw = false
            };

            _profileText = new Font(_arial, 20, FontStyle.Regular);
            _profileName = new Font(_arial, 32, FontStyle.Regular);
            _profileTemplate = Image.Load("Data/Profile/Template/Template.png", new PngDecoder { IgnoreMetadata = true });

            _hgTimes = new Font(_times, 15, FontStyle.Regular);
        }

        private async Task<Image> GetAvatarAsync(CachedMember user, Size size, bool premium)
        {
            var url = user.GetAvatarUrl();
            var gif = user.AvatarHash != null && user.AvatarHash.StartsWith("a_");
            if (url.IsNullOrWhiteSpace())
            {
                var restUser = await user.Guild.GetMemberAsync(user.Id);
                gif = restUser.AvatarHash != null && restUser.AvatarHash.StartsWith("a_");
                url = restUser.GetAvatarUrl();
            }

            try
            {
                return await GetAvatarAsync(url, size, gif, premium);
            }
            catch
            {
                var restUser = await user.Guild.GetMemberAsync(user.Id);
                gif = restUser.AvatarHash != null && restUser.AvatarHash.StartsWith("a_");
                url = restUser.GetAvatarUrl();
                return await GetAvatarAsync(url, size, gif, premium);
            }
        }

        private async Task<Image> GetAvatarAsync(string imgUrl, Size size, bool isGif, bool premium)
        {
            try
            {
                var avatar = await _client.GetStreamAsync(imgUrl);
                var response = avatar.ToEditable();
                response.Position = 0;
                var radius = (int)Math.Ceiling((size.Width * Math.PI) / (2 * Math.PI));
                if (premium && isGif)
                {
                    using var img = await Image.LoadAsync(response, new GifDecoder());
                    using var toReturn = new Image<Rgba64>(Configuration.Default, size.Width, size.Height);
                    for (var i = 0; i < img.Frames.Count; i++)
                    {
                        var x = img.Frames.CloneFrame(i);
                        x.Mutate(z => z.ConvertToAvatar(size, radius));
                        toReturn.Frames.InsertFrame(i, x.Frames[0]);
                    }
                    toReturn.Frames.RemoveFrame(toReturn.Frames.Count - 1);
                    return toReturn.Clone(x => x.Resize(size.Width, size.Height));
                }
                if (isGif)
                {
                    using var img = await Image.LoadAsync(response, new GifDecoder());
                    return img.Clone(x => x.ConvertToAvatar(size, radius));
                }
                else
                {
                    using var img = await Image.LoadAsync(response, new PngDecoder());
                    return img.Clone(x => x.ConvertToAvatar(size, radius));
                }
            }
            catch
            {
                var avatar = await _client.GetStreamAsync("https://i.imgur.com/kI1RXQZ.png");
                var response = avatar.ToEditable();
                response.Position = 0;
                var radius = (int)Math.Ceiling((size.Width * Math.PI) / (2 * Math.PI));
                using var img = await Image.LoadAsync(response, new PngDecoder());
                return img.Clone(x => x.ConvertToAvatar(size, radius));
            }
        }
    }
}