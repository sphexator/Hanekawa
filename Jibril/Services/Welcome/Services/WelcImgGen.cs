using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jibril.Services.Welcome.Services
{
    public class WelcImgGen
    {
        public static void WelcomeImageGenerator(SocketGuildUser user)
        {
            var randomString = RandomStringGenerator.StringGenerator();

            Random rand = new Random();
            var randomImage = rand.Next(Images.Welcome.Length);
            var filetoLoad = Images.Welcome[randomImage];

            using (var img = Image.Load(filetoLoad))
            {
                PathBuilder pathBuilder = new PathBuilder();
                pathBuilder.SetOrigin(new PointF(95,46));
                pathBuilder.AddBezier(new PointF());
                // bottom height is         96
                // top part is              46
                // Start of the name is at  95
                // End of the name is at    388
            }
        }
    }
}
