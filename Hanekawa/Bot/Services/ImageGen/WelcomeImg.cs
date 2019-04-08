using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> WelcomeBuilder(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var img = await GetBanner(user.Guild.Id))
            {
                var avatar = await _image.GetAvatarAsync(user, new Size(60, 60), 32);
                
                img.Mutate(x => x.DrawImage(avatar, new Point(10, 10), _options));
                try
                {
                    img.Mutate(x => x.DrawText(_centerText, user.GetName().Truncate(15), _welcomeFontRegular, Rgba32.White, new Point(245, 46)));
                }
                catch
                {
                    img.Mutate(x => x.DrawText(_centerText, "Bad Name", _welcomeFontRegular, Rgba32.White, new Point(245, 46)));
                }

                img.Save(stream, new PngEncoder());
            }
            return stream;
        }

        private async Task<Image<Rgba32>> GetBanner(ulong guildId)
        {
            var list = await _db.WelcomeBanners.Where(x => x.GuildId == guildId).ToListAsync();
            if (list.Count == 0) return _welcomeTemplate;
            using (var img = Image.Load(await _client.GetStreamAsync(list[_random.Next(list.Count)].Url)))
            {
                img.Mutate(x => x.Resize(600, 78));
                return img.Clone();
            }
        }
    }
}