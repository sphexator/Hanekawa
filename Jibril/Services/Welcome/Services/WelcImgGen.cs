using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Welcome.Services
{
    public class WelcImgGen
    {
        public static async Task<string> WelcomeImageGeneratorAsync(SocketGuildUser user)
        {
            var randomString = RandomStringGenerator.StringGenerator();
            var avatarToLoad = await ImageGenerator.AvatarGenerator(user, randomString);
            var filePath = $"Data/Welcome/Cache/Banner/{randomString}.png";
            Random rand = new Random();
            var randomImage = rand.Next(Images.Welcome.Length);
            var filetoLoad = Images.Welcome[randomImage];
            
            using (var img = Image.Load(filetoLoad))
            {
                PathBuilder pathBuilder = new PathBuilder();
                pathBuilder.SetOrigin(new PointF(95, 46));
                pathBuilder.AddBezier(new PointF(95, 46), new PointF(241, 71), new PointF(388, 96));
                // bottom Height            96
                // top Height               46
                // Start Width              95
                // End Width                388

                IPath path = pathBuilder.Build();
                var avatar = Image.Load(avatarToLoad);
                var font = SystemFonts.CreateFont("Times New Roman", 34, FontStyle.Regular);
                var text = user.Username;
                img.Mutate(ctx => ctx
                    .DrawText(text, font, Rgba32.White, path, new TextGraphicsOptions(true) // draw the text along the path wrapping at the end of the line
                    {
                        WrapTextWidth = path.Length
                    }));
                img.Mutate(ctx => ctx.DrawImage(avatar, new Size(80, 80), new Point(10, 10), GraphicsOptions.Default));
                img.Save(filePath);
                img.Dispose();
            }
            return filePath;
        }
    }
}
