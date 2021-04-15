using System;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Quartz.Util;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Bot.Service.ImageGeneration
{
    public partial class ImageGenerationService : INService
    {
        private readonly IServiceProvider _provider;
        private readonly Hanekawa _bot;
        private readonly ExpService _experience;
        private readonly HttpClient _http;
        private readonly Random _random;

        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly FontFamily _times;
        
        private readonly Image _profileTemplate;
        private readonly Image _welcomeTemplate;
        private readonly WelcomeBanner _defWelcomeBanner;
        
        private readonly GraphicsOptions _options = new GraphicsOptions
        {
            Antialias = true
        };
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

        public ImageGenerationService(ExpService experience, Hanekawa bot, IServiceProvider provider, HttpClient http, Random random)
        {
            _experience = experience;
            _bot = bot;
            _provider = provider;
            _http = http;
            _random = random;
            _fonts = new FontCollection();

            _times = _fonts.Install("Data/Fonts/TIMES.TTF");
            _arial = _fonts.Install("Data/Fonts/ARIAL.TTF");

            _welcomeTemplate = Image.Load("Data/Welcome/Default.png", new PngDecoder {IgnoreMetadata = true});
            _profileTemplate = Image.Load("Data/Profile/Template/Template.png", new PngDecoder {IgnoreMetadata = true});
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
        }
        
        private async Task<Image> GetAvatarAsync(IMember user, Size size, bool premium)
        {
            var url = user.GetAvatarUrl();
            var gif = user.AvatarHash != null && user.AvatarHash.StartsWith("a_");
            if (url.IsNullOrWhiteSpace())
            {
                var restUser = await _bot.GetOrFetchMemberAsync(user.GuildId, user.Id);
                gif = restUser.AvatarHash != null && restUser.AvatarHash.StartsWith("a_");
                url = restUser.GetAvatarUrl();
            }
            try
            {
                return await GetAvatarAsync(url, size, gif, premium);
            }
            catch
            {
                var restUser = await _bot.GetOrFetchMemberAsync(user.GuildId, user.Id);
                gif = restUser.AvatarHash != null && restUser.AvatarHash.StartsWith("a_");
                url = restUser.GetAvatarUrl();
                return await GetAvatarAsync(url, size, gif, premium);
            }
        }

        private async Task<Image> GetAvatarAsync(string imgUrl, Size size, bool isGif, bool premium)
        {
            try
            {
                var avatar = await _http.GetStreamAsync(imgUrl);
                var response = avatar.ToEditable(10);
                response.Position = 0;
                var radius = (int)Math.Ceiling((size.Width * Math.PI) / (2 * Math.PI));
                if (premium && isGif)
                {
                    using var img = await Image.LoadAsync(response, new GifDecoder());
                    using var toReturn = new Image<Rgba64>(Configuration.Default, size.Width, size.Height);
                    for (var i = 0; i < img.Frames.Count; i++)
                    {
                        AddFrames(size, img, i, radius, toReturn);
                    }
                    toReturn.Frames.RemoveFrame(toReturn.Frames.Count - 1);
                    return toReturn.Clone(x => x.Resize(size.Width, size.Height));
                }
                else
                {
                    using var img = await Image.LoadAsync(response, new PngDecoder());
                    return img.Clone(x => x.ConvertToAvatar(size, radius));
                }
            }
            catch
            {
                var avatar = await _http.GetStreamAsync("https://i.imgur.com/kI1RXQZ.png");
                var response = avatar.ToEditable();
                response.Position = 0;
                var radius = (int)Math.Ceiling((size.Width * Math.PI) / (2 * Math.PI));
                using var img = await Image.LoadAsync(response, new PngDecoder());
                return img.Clone(x => x.ConvertToAvatar(size, radius));
            }
        }

        private static void AddFrames(Size size, Image img, int i, int radius, Image<Rgba64> toReturn)
        {
            var x = img.Frames.CloneFrame(i);
            x.Mutate(z => z.ConvertToAvatar(size, radius));
            toReturn.Frames.InsertFrame(i, x.Frames[0]);
        }
    }
}