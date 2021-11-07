using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Entities.Config;
using Hanekawa.Infrastructure;
using Hanekawa.WebUI.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Hanekawa.WebUI.Bot.Service.ImageGeneration
{
    public partial class ImageGenerationService
    { 
        public async Task<Tuple<Stream, bool>> WelcomeAsync(IMember user, DbService db, bool premium)
        {
            var stream = new MemoryStream();
            bool isGif;
            var (img, welcomeBanner) = await GetBannerAsync(user.GuildId, db, premium);
            using (img)
            {
                var avatar = await GetAvatarAsync(user, new Size(welcomeBanner.AvatarSize, welcomeBanner.AvatarSize), premium);
                var username = user.Name.Truncate(15);
                if (premium && img.Frames.Count > 1)
                {
                    await AnimateBanner(img, avatar, user.Name, welcomeBanner.AviPlaceX, welcomeBanner.AviPlaceY, welcomeBanner.TextSize,
                        welcomeBanner.TextPlaceX, welcomeBanner.TextPlaceY).SaveAsync(stream, new GifEncoder());
                    isGif = true;
                }
                else
                {
                    // Text placement: new Point(245, 40)
                    img.Mutate(x => x.DrawImage(avatar, new Point(welcomeBanner.AviPlaceX, welcomeBanner.AviPlaceY), _options));
                    try
                    {
                        img.Mutate(
                            x => x.DrawText(_centerText, username, new Font(_fonts.Find("times"), welcomeBanner.TextSize, FontStyle.Regular), Color.White,
                                new Point(welcomeBanner.TextPlaceX, welcomeBanner.TextPlaceY)));
                    }
                    catch
                    {
                        username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(username));
                        img.Mutate(
                            x => x.DrawText(_centerText, username, new Font(_fonts.Find("times"), welcomeBanner.TextSize, FontStyle.Regular), Color.White,
                                new Point(welcomeBanner.TextPlaceX, welcomeBanner.TextPlaceY)));
                    }

                    await img.SaveAsync(stream, new PngEncoder());
                    isGif = false;
                }
            }

            return new Tuple<Stream, bool>(stream, isGif);
        }

        public async Task<Tuple<Stream, bool>> WelcomeAsync(IMember user, string url, int aviSize, int aviX, int aviY, int textSize, int textX, int textY, bool premium)
        {
            var stream = new MemoryStream();
            bool isGif;
            using var img = await GetBannerAsync(url, premium);
            var avatar = await GetAvatarAsync(user, new Size(aviSize, aviSize), premium);
            var username = user.Name.Truncate(15);
            if (premium && (img.Frames.Count > 1 || avatar.Frames.Count > 1))
            {
                await AnimateBanner(img, avatar, user.Name, aviX, aviY, textSize, textX, textY).SaveAsync(stream, new GifEncoder());
                isGif = true;
            }
            else
            {
                img.Mutate(x => x.DrawImage(avatar, new Point(aviX, aviY), _options));
                try
                {
                    img.Mutate(
                        x => x.DrawText(_centerText, username, new Font(_fonts.Find("times"), textSize, FontStyle.Regular), Color.White,
                            new Point(textX, textY)));
                }
                catch
                {
                    username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(username));
                    img.Mutate(
                        x => x.DrawText(_centerText, username, new Font(_fonts.Find("times"), textSize, FontStyle.Regular), Color.White,
                            new Point(textX, textY)));
                }

                await img.SaveAsync(stream, new PngEncoder());
                isGif = false;
            }

            return new Tuple<Stream, bool>(stream, isGif);
        }

        private Image AnimateBanner(Image banner, Image avatar, string name, int aviX, int aviY, int textSize, int textX, int textY)
        {
            var color = new Rgba32(255, 255, 255, 1f);
            var aviIterate = 0;
            using var gif = new Image<Rgba64>(Configuration.Default, banner.Width, banner.Height);
            for (var i = 0; i < banner.Frames.Count; i++)
            {
                using var img = banner.Frames.CloneFrame(i);
                img.Mutate(x => x.DrawImage(avatar.Frames.CloneFrame(aviIterate), new Point(aviX, aviY), 1)); 
                img.Mutate(x => x.DrawText(_centerText, name, new Font(_fonts.Find("times"), textSize, FontStyle.Regular), color, new Point(textX, textY)));
                gif.Frames.InsertFrame(i, img.Frames[0]);
                if (avatar.Frames.Count > aviIterate + 1) aviIterate++;
            }
            gif.Frames.RemoveFrame(gif.Frames.Count - 1);
            gif.Metadata.GetGifMetadata().RepeatCount = 0;
            return gif.Clone();
        }

        private async Task<Tuple<Image, WelcomeBanner>> GetBannerAsync(ulong guildId, DbService db, bool premium)
        {
            var list = await db.WelcomeBanners.Where(x => x.GuildId == guildId).ToListAsync();
            if (list.Count == 0) return new Tuple<Image, WelcomeBanner>(_welcomeTemplate, _defWelcomeBanner);
            var backgroundRaw = list[_random.Next(list.Count)];
            var background = await _http.CreateClient().GetStreamAsync(backgroundRaw.Url);
            var response = background.ToEditable(10);
            response.Position = 0;
            var file = response.GetKnownFileType();
            int width;
            int height;
            if (premium && file == FileType.Gif)
            {
                using var img = await Image.LoadAsync(response, new GifDecoder());
                width = img.Width;
                height = img.Height;
                return new Tuple<Image, WelcomeBanner>(img.Clone(x => x.Resize(width, height)), backgroundRaw);
            }
            else
            {
                using var img = await Image.LoadAsync(response, new PngDecoder());
                width = img.Width;
                height = img.Height;
                return new Tuple<Image, WelcomeBanner>(img.Clone(x => x.Resize(width, height)), backgroundRaw);
            }
        }

        private async Task<Image> GetBannerAsync(string url, bool premium)
        {
            var background = await _http.CreateClient().GetStreamAsync(url);
            var response = background.ToEditable(10);
            response.Position = 0;
            var file = response.GetKnownFileType();
            int width;
            int height;
            if (premium && file == FileType.Gif)
            {
                using var img = await Image.LoadAsync(response, new GifDecoder());
                width = img.Width;
                height = img.Height;
                return img.Clone(x => x.Resize(width, height));
            }
            else
            {
                using var img = await Image.LoadAsync(response, new PngDecoder());
                width = img.Width;
                height = img.Height;
                return img.Clone(x => x.Resize(width, height));
            }
        }
    }
}