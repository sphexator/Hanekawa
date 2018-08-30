using Discord.WebSocket;
using Hanekawa.Services.Entities.Tables;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Numerics;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;

namespace Hanekawa.Extensions
{
    public static class ImageExtension
    {
        private static IImageProcessingContext<Rgba32> ConvertToAvatar(
            this IImageProcessingContext<Rgba32> processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).Apply(i => ApplyRoundedCorners(i, cornerRadius));
        }

        public static Image<Rgba32> CloneAndConvertToAvatarWithoutApply(this Image<Rgba32> image, Size size,
            float cornerRadius)
        {
            var result = image.Clone(
                ctx => ctx.Resize(
                    new ResizeOptions
                    {
                        Size = size,
                        Mode = ResizeMode.Crop
                    }));

            ApplyRoundedCorners(result, cornerRadius);
            return result;
        }

        private static void ApplyRoundedCorners(Image<Rgba32> img, float cornerRadius)
        {
            var corners = BuildCorners(img.Width, img.Height, cornerRadius);

            var graphicOptions = new GraphicsOptions(true)
            {
                BlenderMode = PixelBlenderMode.Src
            };
            img.Mutate(x => x.Fill(graphicOptions, Rgba32.Transparent, corners));
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            var cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var center = new Vector2(imageWidth / 2F, imageHeight / 2F);

            var rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        public static IImageProcessingContext<Rgba32> ApplyProfileText(
            this IImageProcessingContext<Rgba32> processingContext, Account userData, SocketGuildUser user,
            uint xpToLevelUp)
        {
            var statFont = SystemFonts.CreateFont("Good Times Rg", 9, FontStyle.Regular);
            var nameFont = SystemFonts.CreateFont("Good Times Rg", 12, FontStyle.Regular);
            var classFont = SystemFonts.CreateFont("Good Times Rg", 8, FontStyle.Regular);
            var shipFont = SystemFonts.CreateFont("Good Times Rg", 5, FontStyle.Regular);

            var gdclassFont = GoodFont(shipFont, $"{userData.Class}", 88, 16);
            var usernameFont = GoodFont(nameFont, $"{user.GetName()}", 170, 30);

            const string missionstr = "Missions completed";
            const string commanderstr = "Commander";
            const string fleetstr = "Fleet";
            const string npckillstr = "NPC Kills";
            const string damagestr = "Damage";
            const string healthstr = "Health";
            const string creditstr = "Credit";
            const string totalexpstr = "TOTAL EXP";
            const string expstr = "EXP";
            const string levelstr = "LEVEL";
            // Paths
            var levelPath = new PathBuilder().AddLine(new Point(114, 132), new Point(284, 132));
            var expPath = new PathBuilder().AddLine(new Point(114, 144), new Point(284, 144));
            var totalexpPath = new PathBuilder().AddLine(new Point(114, 156), new Point(284, 156));
            var creditPath = new PathBuilder().AddLine(new Point(114, 168), new Point(284, 168));

            var healthPath = new PathBuilder().AddLine(new Point(114, 191), new Point(284, 191));
            var damagePath = new PathBuilder().AddLine(new Point(114, 201), new Point(284, 201));
            var npckillPath = new PathBuilder().AddLine(new Point(114, 211), new Point(284, 211));
            var fleetPath = new PathBuilder().AddLine(new Point(114, 221), new Point(284, 221));
            var commanderPath = new PathBuilder().AddLine(new Point(114, 231), new Point(284, 231));
            var missionPath = new PathBuilder().AddLine(new Point(114, 241), new Point(284, 241));

            //Apply lines
            processingContext.Draw(Rgba32.DarkGray, 1, levelPath.Build()); // Level area       
            processingContext.Draw(Rgba32.DarkGray, 1, expPath.Build()); //Exp area
            processingContext.Draw(Rgba32.DarkGray, 1, totalexpPath.Build()); //Total exp area
            processingContext.Draw(Rgba32.DarkGray, 1, creditPath.Build()); // Credit area
            // Game Info
            processingContext.Draw(Rgba32.DarkGray, 1, healthPath.Build()); // Health area
            processingContext.Draw(Rgba32.DarkGray, 1, damagePath.Build()); // Damage area
            processingContext.Draw(Rgba32.DarkGray, 1, npckillPath.Build()); // NPC kills area
            processingContext.Draw(Rgba32.DarkGray, 1, fleetPath.Build()); // Fleet area
            processingContext.Draw(Rgba32.DarkGray, 1, commanderPath.Build()); // Commander area
            processingContext.Draw(Rgba32.DarkGray, 1, missionPath.Build()); // Mission Completed area

            var optionsName = new TextGraphicsOptions
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                WrapTextWidth = user.GetName().Length
            };

            var optionsLeft = new TextGraphicsOptions
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var optionsRight = new TextGraphicsOptions
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            processingContext.DrawText(optionsName, $"{user.GetName()}", usernameFont, Rgba32.Black,
                new Point(198, 90));

            processingContext.DrawText(optionsLeft, levelstr, statFont, Rgba32.Black, new Point(114, 120));
            processingContext.DrawText(optionsRight, $"{userData.Level}", statFont, Rgba32.Black, new Point(284, 120));

            processingContext.DrawText(optionsRight, $"{userData.Exp}/{xpToLevelUp}", statFont, Rgba32.Black,
                new Point(284, 132));
            processingContext.DrawText(optionsLeft, expstr, statFont, Rgba32.Black, new Point(114, 132));

            processingContext.DrawText(optionsLeft, totalexpstr, statFont, Rgba32.Black, new Point(114, 144));
            processingContext.DrawText(optionsRight, $"{userData.TotalExp}", statFont, Rgba32.Black,
                new Point(284, 144));

            processingContext.DrawText(optionsLeft, creditstr, statFont, Rgba32.Black, new Point(114, 156));
            processingContext.DrawText(optionsRight, $"{userData.Credit}", statFont, Rgba32.Black, new Point(284, 156));

            processingContext.DrawText(optionsLeft, healthstr, classFont, Rgba32.Black, new Point(114, 180));
            processingContext.DrawText(optionsRight, "100/100", classFont, Rgba32.Black, new Point(284, 180));

            processingContext.DrawText(optionsLeft, damagestr, classFont, Rgba32.Black, new Point(114, 190));
            processingContext.DrawText(optionsRight, "0", classFont, Rgba32.Black, new Point(284, 190));

            processingContext.DrawText(optionsLeft, npckillstr, classFont, Rgba32.Black, new Point(114, 200));
            processingContext.DrawText(optionsRight, $"{userData.GameKillAmount}", classFont, Rgba32.Black,
                new Point(284, 200));

            processingContext.DrawText(optionsLeft, fleetstr, classFont, Rgba32.Black, new Point(114, 210));
            processingContext.DrawText(optionsRight, "N/A", classFont, Rgba32.Black, new Point(284, 210));

            processingContext.DrawText(optionsLeft, commanderstr, classFont, Rgba32.Black, new Point(114, 220));
            processingContext.DrawText(optionsRight, "N/A", classFont, Rgba32.Black, new Point(284, 220));

            processingContext.DrawText(optionsLeft, missionstr, classFont, Rgba32.Black, new Point(114, 230));
            processingContext.DrawText(optionsRight, "0", classFont, Rgba32.Black, new Point(284, 230));

            processingContext.DrawText(optionsName, $"TBD", gdclassFont, Rgba32.Black, new PointF(48, 278));
            return processingContext;
        }

        private static Font GoodFont(Font font, string text, int width, int height)
        {
            var size = TextMeasurer.Measure(text, new RendererOptions(font));
            var scalingFactor = Math.Min(width / size.Width, height / size.Height);
            var scaledFont = new Font(font, scalingFactor * font.Size);
            return scaledFont;
        }
    }
}