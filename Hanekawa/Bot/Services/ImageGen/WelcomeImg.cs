using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> WelcomeBuilder(CachedMember user, DbService db)
        {
            var stream = new MemoryStream();
            using (var img = await GetBanner(user.Guild.Id.RawValue, db))
            {
                var avatar = await GetAvatarAsync(user, new Size(60, 60), 32);
                img.Mutate(x => x.DrawImage(avatar, new Point(10, 10), _options));

                var username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.DisplayName.Truncate(15)));
                img.Mutate(
                    x => x.DrawText(_centerText, username, _welcomeFontRegular, SixLabors.ImageSharp.Color.White, new Point(245, 46)));
                await img.SaveAsync(stream, new PngEncoder());
            }

            return stream;
        }

        public async Task<Stream> WelcomeBuilder(CachedMember user, string url)
        {
            var stream = new MemoryStream();
            using (var img = await GetBanner(url))
            {
                var avatar = await GetAvatarAsync(user, new Size(60, 60), 32);
                img.Mutate(x => x.DrawImage(avatar, new Point(10, 10), _options));

                var username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.DisplayName.Truncate(15)));
                img.Mutate(
                    x => x.DrawText(_centerText, username, _welcomeFontRegular, SixLabors.ImageSharp.Color.White, new Point(245, 46)));
                await img.SaveAsync(stream, new PngEncoder());
            }

            return stream;
        }

        private async Task<Image> GetBanner(ulong guildId, DbService db)
        {
            var list = await db.WelcomeBanners.Where(x => x.GuildId == guildId).ToListAsync();
            if (list.Count == 0) return _welcomeTemplate;
            var response = await _client.GetStreamAsync(list[_random.Next(list.Count)].Url);
            using var img = await Image.LoadAsync(response.ToEditable(), new PngDecoder());
            return img.Clone(x => x.Resize(600, 78));
        }

        private async Task<Image> GetBanner(string url)
        {
            var response = await _client.GetStreamAsync(url);
            using var img = await Image.LoadAsync(response.ToEditable(), new PngDecoder());
            return img.Clone(x => x.Resize(600, 78));
        }
    }
}