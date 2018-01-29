using System;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Fleet.Services;
using Jibril.Modules.Game.Services;
using Jibril.Services.Level.Lists;
using Jibril.Services.Level.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using Image = SixLabors.ImageSharp.Image;

namespace Jibril.Modules.Profile.Services
{
    public static class ApplyText
    {
        public static string ApplyTextToProfile(string filepath, SocketUser user, string randomString,
            UserData userData, GameStatus gameData)
        {
            var finalPath = $"Data/Images/Profile/Cache/{randomString}Final.png";
            var statFont = SystemFonts.CreateFont("Good Times Rg", 9, FontStyle.Regular);
            var nameFont = SystemFonts.CreateFont("Good Times Rg", 12, FontStyle.Regular);
            var classFont = SystemFonts.CreateFont("Good Times Rg", 8, FontStyle.Regular);
            var shipFont = SystemFonts.CreateFont("Good Times Rg", 5, FontStyle.Regular);

            var ap = BaseStats.BaseAttackPoint(userData.Level, userData.ShipClass);
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);

            var gdclassFont = GoodFont(shipFont, $"{userData.ShipClass}", 5, 88, 16);
            var NameFont = GoodFont(nameFont, $"{user.Username}", 5, 170, 30);

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
            using (var img = Image.Load(filepath))
            {
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


                var username = usernamePath.Build();
                var level = levelPath.Build();
                var exp = expPath.Build();
                var totalexp = totalexpPath.Build();
                var credit = creditPath.Build();
                var health = healthPath.Build();
                var damage = damagePath.Build();
                var npckill = npckillPath.Build();
                var fleet = fleetPath.Build();
                var commander = commanderPath.Build();
                var mission = missionPath.Build();

                //Dynamic variables
                var fleetName = CheckFleetMembership(user);
                var fleetCommander = GetFleetCommander(user, fleetName).First().ToString();

                //Apply lines
                img.Mutate(x => x
                    .Draw(Rgba32.DarkGray, 1, level) // Level area       
                    .Draw(Rgba32.DarkGray, 1, exp) //Exp area
                    .Draw(Rgba32.DarkGray, 1, totalexp) //Total exp area
                    .Draw(Rgba32.DarkGray, 1, credit) // Credit area
                    // Game Info
                    .Draw(Rgba32.DarkGray, 1, health) // Health area
                    .Draw(Rgba32.DarkGray, 1, damage) // Damage area
                    .Draw(Rgba32.DarkGray, 1, npckill) // NPC kills area
                    .Draw(Rgba32.DarkGray, 1, fleet) // Fleet area
                    .Draw(Rgba32.DarkGray, 1, commander) // Commander area
                    .Draw(Rgba32.DarkGray, 1, mission)); // Mission Completed area

                img.Mutate(x => x
                    .DrawText($"{user.Username}", NameFont, Rgba32.Black, username, new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = username.Length
                    })
                    .DrawText(levelstr, statFont, Rgba32.Black, new Point(114, 120), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = level.Length
                    })
                    .DrawText(userData.Level.ToString(), statFont, Rgba32.Black, new Point(284, 120),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })
                    .DrawText($"{userData.Xp}/{xpToLevelUp}", statFont, Rgba32.Black, new Point(284, 132),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })
                    .DrawText(expstr, statFont, Rgba32.Black, new Point(114, 132), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = exp.Length
                    })
                    .DrawText(totalexpstr, statFont, Rgba32.Black, new Point(114, 144), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = totalexp.Length
                    })
                    .DrawText($"{userData.Total_xp}", statFont, Rgba32.Black, new Point(284, 144),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })
                    .DrawText(creditstr, statFont, Rgba32.Black, new Point(114, 156), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = credit.Length
                    })
                    .DrawText($"{userData.Tokens}", statFont, Rgba32.Black, new Point(284, 156),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        }));
                try
                {
                    img.Mutate(x => x
                        .DrawText(healthstr, classFont, Rgba32.Black, new Point(114, 180), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Antialias = true,
                            ApplyKerning = true,
                            WrapTextWidth = health.Length
                        })
                        .DrawText($"{gameData.Health - gameData.Damagetaken}/{gameData.Health}", classFont,
                            Rgba32.Black,
                            new Point(284, 180), new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Antialias = true,
                                ApplyKerning = true
                            })
                        .DrawText(damagestr, classFont, Rgba32.Black, new Point(114, 190), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Antialias = true,
                            ApplyKerning = true,
                            WrapTextWidth = damage.Length
                        })
                        .DrawText($"{ap}", classFont, Rgba32.Black, new Point(284, 190), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })
                        .DrawText(npckillstr, classFont, Rgba32.Black, new Point(114, 200),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Antialias = true,
                                ApplyKerning = true,
                                WrapTextWidth = npckill.Length
                            })
                        .DrawText($"{gameData.KillAmount}", classFont, Rgba32.Black, new Point(284, 200),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Antialias = true,
                                ApplyKerning = true
                            })
                        .DrawText(fleetstr, classFont, Rgba32.Black, new Point(114, 210), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Antialias = true,
                            ApplyKerning = true,
                            WrapTextWidth = fleet.Length
                        })
                        .DrawText($"{fleetName}", classFont, Rgba32.Black, new Point(284, 210),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Antialias = true,
                                ApplyKerning = true
                            })
                        .DrawText(commanderstr, classFont, Rgba32.Black, new Point(114, 220),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Antialias = true,
                                ApplyKerning = true,
                                WrapTextWidth = commander.Length
                            })
                        .DrawText($"{fleetCommander}", classFont, Rgba32.Black, new Point(284, 220),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Antialias = true,
                                ApplyKerning = true
                            })
                        .DrawText(missionstr, classFont, Rgba32.Black, new Point(114, 230),
                            new TextGraphicsOptions(true)
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Antialias = true,
                                ApplyKerning = true,
                                WrapTextWidth = mission.Length
                            })
                        .DrawText($"0", classFont, Rgba32.Black, new Point(284, 230), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }

                img.Mutate(x => x
                    .DrawText($"{userData.ShipClass}", gdclassFont, Rgba32.Black, new PointF(50, 280),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Antialias = true,
                            ApplyKerning = true
                        }));
                img.Save(finalPath);
                return finalPath;
            }
        }

        private static Font GoodFont(Font font, string text, float padding, int width, int height)
        {
            var size = TextMeasurer.Measure(text, new RendererOptions(font));
            var scalingFactor = Math.Min(width / size.Width, height / size.Height);
            var scaledFont = new Font(font, scalingFactor * font.Size);
            return scaledFont;
        }

        private static string CheckFleetMembership(IUser user)
        {
            var fleetName = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            if (fleetName == null) return "N/A";
            return fleetName.Equals("o", StringComparison.InvariantCultureIgnoreCase) ? "N/A" : fleetName;
        }

        private static string GetFleetCommander(IUser user, string fleet)
        {
            if (fleet.Equals("N/A", StringComparison.InvariantCultureIgnoreCase)) return "N/A";
            var commander = FleetNormDb.GetLeader(user, fleet).ToString();
            return commander ?? "N/A";
        }
    }
}