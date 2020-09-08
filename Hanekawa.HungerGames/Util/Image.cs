using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.HungerGames.Entities.Internal;
using Hanekawa.HungerGames.Entities.User;

namespace Hanekawa.HungerGames.Util
{
    internal class Image : IRequired
    {
        private readonly HttpClient _client;
        internal Image(HttpClient client) => _client = client;

        internal async Task<int> DetermineImageSizeAsync(IEnumerable<HungerGameProfile> profiles)
        {
            var size = int.MaxValue;
            foreach (var x in profiles)
            {
                using (var image = SixLabors.ImageSharp.Image.Load(await _client.GetStreamAsync(x.Avatar)))
                {
                    if (image.Width < size) size = image.Width;
                    if (image.Height < size) size = image.Height;
                }
            }

            return size;
        }
    }
}
