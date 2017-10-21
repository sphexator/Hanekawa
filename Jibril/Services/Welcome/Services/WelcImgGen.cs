using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;

namespace Jibril.Services.Welcome.Services
{
    public class WelcImgGen
    {
        public static string WelcomeImageGeneratorAsync(SocketGuildUser user, string avatarToLoad)
        {
            //var randomString = RandomStringGenerator.StringGenerator();
            var filePath = @"Data\Images\Welcome\Cache\Banner\welcome.png";

            Random rand = new Random();
            var randomImage = rand.Next(Images.Welcome.Length);
            var filetoLoad = Images.Welcome[randomImage];
            
            using (var img = Image.Load(filetoLoad))
            {
                PathBuilder pathBuilder = new PathBuilder();
                // pathBuilder.SetOrigin(new Point(95, 46));
                pathBuilder.AddLine((new Point(94, 57)), (new Point(390, 57)));
                // pathBuilder.AddLine((new Point(388, 46)), (new Point(338, 96)));
                // pathBuilder.AddLine((new Point(388, 96)), (new Point(95, 96)));
                // pathBuilder.AddLine((new Point(95, 96)), (new Point(95, 46)));
                // pathBuilder.AddBezier(new Point(95, 46), new Point(241, 71), new Point(388, 96));

                // bottom Height            96
                // top Height               46
                // Start Width              95
                // End Width                388

                IPath path = pathBuilder.Build();

                var avatar = Image.Load(avatarToLoad);
                var font = SystemFonts.CreateFont("Times New Roman", 34, FontStyle.Regular);
                var text = user.Username.ToString();

                img.Mutate(ctx => ctx
                    .DrawImage(avatar, new Size(60, 60), new Point(10, 10), GraphicsOptions.Default)
                    .DrawText(text.Truncate(15), font, Rgba32.White, path, new TextGraphicsOptions(true) // draw the text along the path wrapping at the end of the line
                    {
                        WrapTextWidth = path.Length,
                        Antialias = true,
                        ApplyKerning = true,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }));
                img.Save(filePath);
            }
            return filePath;
        }
    }
}
