using System;
using System.Net.Http;
using Hanekawa.Database.Tables.Config;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Hanekawa.Bot.Service.ImageGeneration
{
    public partial class ImageGenerationService : INService
    {
        private readonly IServiceProvider _provider;
        private readonly Hanekawa _bot;
        private readonly HttpClient _http;
        private readonly Random _random;

        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly FontFamily _times;
        
        private readonly Image _welcomeTemplate;
        private readonly WelcomeBanner _defWelcomeBanner;
        
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
    }
}