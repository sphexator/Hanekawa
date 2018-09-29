using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Profile.Entities;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Addons.Profile
{
    public class ProfileGenerator : IDisposable
    {
        public void Dispose()
        {
        }

        public async Task<Stream> Create(DbService db, string name, string avatar, ulong userId, ulong guildId,
            FileType type = FileType.Png)
        {
            using (var client = new HttpClient())
            using (var image = Image.Load(await client.GetStreamAsync("")))
            {
                var result = new MemoryStream();
                image.Mutate(async x => await x.ApplyTextAsync(db, userId, guildId));
                image.Save(result, DetectFileType(type));
                return result;
            }
        }

        private static IImageEncoder DetectFileType(FileType type)
        {
            IImageEncoder encoder = null;
            switch (type)
            {
                case FileType.Gif:
                    encoder = new GifEncoder();
                    break;
                case FileType.Jpeg:
                    encoder = new JpegEncoder();
                    break;
                case FileType.Png:
                    encoder = new PngEncoder();
                    break;
            }

            return encoder;
        }

        private async Task<string> GetBackground(DbService db)
        {
            return (await db.Backgrounds.OrderBy(x => new Random().Next()).FirstOrDefaultAsync()).BackgroundUrl;
        }
    }
}