using Discord.WebSocket;
using Jibril.Services.Entities.Tables;
using Jibril.Services.Level.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;

namespace Jibril.Extensions
{
    public static class ImageExtension
    {
        private static readonly Calculate Calculate;

        public static IImageProcessingContext<Rgba32> ConvertToAvatar(
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


            img.Mutate(x => x.Fill(Rgba32.Transparent, corners));
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularePolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);
            var cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        public static IImageProcessingContext<Rgba32> ApplyProfileText(this IImageProcessingContext<Rgba32> processingContext, Account userData, SocketGuildUser user)
        {
            var statFont = SystemFonts.CreateFont("Good Times Rg", 9, FontStyle.Regular);
            var nameFont = SystemFonts.CreateFont("Good Times Rg", 12, FontStyle.Regular);
            var classFont = SystemFonts.CreateFont("Good Times Rg", 8, FontStyle.Regular);
            var shipFont = SystemFonts.CreateFont("Good Times Rg", 5, FontStyle.Regular);

            var xpToLevelUp = Calculate.GetNextLevelRequirement(userData.Level);

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
            var usernamePath = new PathBuilder();
            var levelPath = new PathBuilder();
            var expPath = new PathBuilder();
            var totalexpPath = new PathBuilder();
            var creditPath = new PathBuilder();
            var healthPath = new PathBuilder();
            var damagePath = new PathBuilder();
            var npckillPath = new PathBuilder();
            var fleetPath = new PathBuilder();
            var commanderPath = new PathBuilder();
            var missionPath = new PathBuilder();

            usernamePath.AddLine(new Point(114, 105), new Point(284, 105));
            levelPath.AddLine(new Point(114, 132), new Point(284, 132));
            expPath.AddLine(new Point(114, 144), new Point(284, 144));
            totalexpPath.AddLine(new Point(114, 156), new Point(284, 156));
            creditPath.AddLine(new Point(114, 168), new Point(284, 168));

            healthPath.AddLine(new Point(114, 191), new Point(284, 191));
            damagePath.AddLine(new Point(114, 201), new Point(284, 201));
            npckillPath.AddLine(new Point(114, 211), new Point(284, 211));
            fleetPath.AddLine(new Point(114, 221), new Point(284, 221));
            commanderPath.AddLine(new Point(114, 231), new Point(284, 231));
            missionPath.AddLine(new Point(114, 241), new Point(284, 241));

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


            processingContext.DrawText($"{user.GetName()}", usernameFont, Rgba32.Black, usernamePath.Build(),
                new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = usernamePath.Build().Length
                });
            processingContext.DrawText(levelstr, statFont, Rgba32.Black, new Point(114, 120), new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Antialias = true,
                ApplyKerning = true,
                WrapTextWidth = levelPath.Build().Length
            });
            processingContext.DrawText(userData.Level.ToString(), statFont, Rgba32.Black, new Point(284, 120),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
            processingContext.DrawText($"{userData.Exp}/{xpToLevelUp}", statFont, Rgba32.Black, new Point(284, 132),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
            processingContext.DrawText(expstr, statFont, Rgba32.Black, new Point(114, 132), new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Antialias = true,
                ApplyKerning = true,
                WrapTextWidth = expPath.Build().Length
            });
            processingContext.DrawText(totalexpstr, statFont, Rgba32.Black, new Point(114, 144), new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Antialias = true,
                ApplyKerning = true,
                WrapTextWidth = totalexpPath.Build().Length
            });
            processingContext.DrawText($"{userData.TotalExp}", statFont, Rgba32.Black, new Point(284, 144),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
            processingContext.DrawText(creditstr, statFont, Rgba32.Black, new Point(114, 156), new TextGraphicsOptions(true)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Antialias = true,
                ApplyKerning = true,
                WrapTextWidth = creditPath.Build().Length
            });
            processingContext.DrawText($"{userData.Credit}", statFont, Rgba32.Black, new Point(284, 156),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
            try
            {
                processingContext.DrawText(healthstr, classFont, Rgba32.Black, new Point(114, 180), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = healthPath.Build().Length
                    });
                processingContext.DrawText("100/100", classFont,
                    Rgba32.Black,
                    new Point(284, 180), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
                processingContext.DrawText(damagestr, classFont, Rgba32.Black, new Point(114, 190),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = damagePath.Build().Length
                    });
                processingContext.DrawText("0", classFont, Rgba32.Black, new Point(284, 190),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
                processingContext.DrawText(npckillstr, classFont, Rgba32.Black, new Point(114, 200),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Antialias = true,
                            ApplyKerning = true,
                            WrapTextWidth = npckillPath.Build().Length
                        });
                processingContext.DrawText($"{userData.GameKillAmount}", classFont, Rgba32.Black, new Point(284, 200),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
                processingContext.DrawText(fleetstr, classFont, Rgba32.Black, new Point(114, 210),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = fleetPath.Build().Length
                    });
                processingContext.DrawText("N/A", classFont, Rgba32.Black, new Point(284, 210),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
                processingContext.DrawText(commanderstr, classFont, Rgba32.Black, new Point(114, 220),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = commanderPath.Build().Length
                    });
                processingContext.DrawText("N/A", classFont, Rgba32.Black, new Point(284, 220),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
                processingContext.DrawText(missionstr, classFont, Rgba32.Black, new Point(114, 230),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = missionPath.Build().Length
                    });
                processingContext.DrawText("0", classFont, Rgba32.Black, new Point(284, 230), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Antialias = true,
                        ApplyKerning = true
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
            processingContext.DrawText($"{userData.Class}", gdclassFont, Rgba32.Black, new PointF(50, 280),
                    new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Antialias = true,
                        ApplyKerning = true
                    });
            return processingContext;
        }

        private static Font GoodFont(Font font, string text, int width, int height)
        {
            var size = TextMeasurer.Measure(text, new RendererOptions(font));
            var scalingFactor = Math.Min(width / size.Width, height / size.Height);
            var scaledFont = new Font(font, scalingFactor * font.Size);
            return scaledFont;
        }

        /*
         // TODO: Add club to profile
        private static string CheckFleetMembership(IUser user)
        {
            var ClubName = ClubDb.UserClubData(user).FirstOrDefault();
            return ClubName == null ? "N/A" : ClubName.ClubName;
        }

        private static string GetFleetCommander(IUser user, string fleet)
        {
            if (fleet.Equals("N/A", StringComparison.InvariantCultureIgnoreCase)) return "N/A";
            var commander = ClubDb.GetClubs().FirstOrDefault(x => x.Name == fleet);
            return commander?.Name ?? "N/A";
        }
        */
    }
}
