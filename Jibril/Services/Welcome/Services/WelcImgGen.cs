using System;
using Discord.WebSocket;
using Humanizer;
using Jibril.Data.Variables;
using Jibril.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Jibril.Services.Welcome.Services
{
    public class WelcImgGen
    {
        public static string WelcomeImageGeneratorAsync(SocketGuildUser user, string avatarToLoad, string randomString)
        {
            var filePath = $"Data/Images/Welcome/Cache/Banner/{randomString}.png";

            var rand = new Random();
            var randomImage = rand.Next(Images.Welcome.Length);
            var filetoLoad = Images.Welcome[randomImage];

            using (var img = Image.Load(filetoLoad))
            {
                var pathBuilder = new PathBuilder();
                pathBuilder.AddLine(new Point(94, 57), new Point(390, 57));

                var avatar = Image.Load(avatarToLoad);
                var font = SystemFonts.CreateFont("Times New Roman", 33, FontStyle.Regular);
                var text = user.Username;

                img.Mutate(ctx => ctx
                    .DrawImage(avatar, new Size(60, 60), new Point(10, 10), GraphicsOptions.Default)
                    .DrawText(text.Truncate(15), font, Rgba32.White, new PointF(245, 51), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Antialias = true,
                        ApplyKerning = true
                    }));
                img.Save(filePath);
            }
            return filePath;
        }
    }
}