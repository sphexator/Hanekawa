using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Bot.Services.ImageGen
{
    public class ImageGenerator
    {
        private readonly HttpClient _client;

        public ImageGenerator(HttpClient client) => _client = client;

        public async Task<Image<Rgba32>> GetAvatarAsync(IUser user, Size size, int radius)
        {
            var response = await _client.GetStreamAsync(user.GetAvatarUrl());
            using (var img = Image.Load(response))
            {
                var avi = img.CloneAndConvertToAvatarWithoutApply(size, radius);
                return avi.Clone();
            }
        }

        public async Task<Image<Rgba32>> GetAvatarAsync(IUser user, Size size)
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