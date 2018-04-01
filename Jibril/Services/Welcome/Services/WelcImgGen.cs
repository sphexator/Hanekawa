using System;
using Discord.WebSocket;
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

                var path = pathBuilder.Build();

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
                    })
                    );
                var effect = rand.Next(1, 14);
                if (effect == 1)
                {
                    img.Mutate(ctx => ctx
                    .Flip(FlipType.Horizontal));
                }
                if (effect == 2)
                {
                    img.Mutate(ctx => ctx
                        .Flip(FlipType.Vertical));
                }
                if (effect == 3)
                {
                    img.Mutate(ctx => ctx
                        .RotateFlip(RotateType.Rotate90, FlipType.Horizontal));
                }
                if (effect == 4)
                {
                    img.Mutate(ctx => ctx
                        .RotateFlip(RotateType.Rotate180, FlipType.Horizontal));
                }
                if (effect == 5)
                {
                    img.Mutate(ctx => ctx
                        .RotateFlip(RotateType.Rotate270, FlipType.Horizontal));
                }
                if (effect == 6)
                {
                    img.Mutate(ctx => ctx
                        .Invert());
                }
                if (effect == 7)
                {
                    img.Mutate(ctx => ctx
                        .OilPaint());
                }
                if (effect == 8)
                {
                    img.Mutate(ctx => ctx
                        .Pixelate());
                }
                if (effect == 8)
                {
                    img.Mutate(ctx => ctx
                        .Vignette());
                }
                if (effect == 9)
                {
                    img.Mutate(ctx => ctx
                        .ColorBlindness(ColorBlindness.Tritanomaly));
                }
                if (effect == 10)
                {
                    img.Mutate(ctx => ctx
                        .ColorBlindness(ColorBlindness.Tritanopia));
                }
                if (effect == 11)
                {
                    img.Mutate(ctx => ctx
                        .ColorBlindness(ColorBlindness.Achromatomaly));
                }
                if (effect == 12)
                {
                    img.Mutate(ctx => ctx
                        .ColorBlindness(ColorBlindness.Deuteranomaly));
                }
                if (effect == 13)
                {
                    img.Mutate(ctx => ctx
                        .ColorBlindness(ColorBlindness.Protanomaly));
                }

                if (effect == 14)
                {
                    //ignore
                }

                img.Save(filePath);
            }
            return filePath;
        }
    }
}