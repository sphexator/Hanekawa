using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> ShipGameBuilder(string pOneAviUrl, string pTwoAviUrl)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load("Data/Game/background.png"))
            {
                // var border = Image.Load(GetBorder());
                // This will be in the future
                var aviOne = await GetAvatarAsync(pOneAviUrl, new Size(126, 126), false, false);
                var aviTwo = await GetAvatarAsync(pTwoAviUrl, new Size(126, 126), false, false);
                img.Mutate(x => x
                    .DrawImage(aviOne, new Point(3, 92), new GraphicsOptions {Antialias = true})
                    .DrawImage(aviTwo, new Point(223, 92), new GraphicsOptions {Antialias = true})
                    .DrawImage(Image.Load("Data/Game/Border/Red-border.png"), new Point(0, 0),
                        new GraphicsOptions {Antialias = true}));
                await img.SaveAsync(stream, new PngEncoder());
            }

            return stream;
        }
    }
}