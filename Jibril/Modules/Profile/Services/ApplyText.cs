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
                var CommanderPath = new PathBuilder();
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
                CommanderPath.AddLine(new Point(114, 231), new Point(284, 231));
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
                var commander = CommanderPath.Build();
                var mission = missionPath.Build();

                var levelstr = "LEVEL";
                var expstr = "EXP";
                var totalexpstr = "TOTAL EXP";
                var creditstr = "Credit";
                var healthstr = "Health";
                var damagestr = "Damage";
                var npckillstr = "NPC Kills";
                var fleetstr = "Fleet";
                var commanderstr = "Commander";
                var missionstr = "Missions completed";

                //Dynamic variables
                var fleetName = CheckFleetMembership(user);
                var fleetCommander = GetFleetCommander(user, fleetName);


                img.Mutate(x => x
                    // User info
                    // Username area
                    .DrawText($"{user.Username}", NameFont, Rgba32.Black, username, new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = username.Length
                    })

                    // Level area
                    .Draw(Rgba32.DarkGray, 1, level)
                    .DrawText(levelstr, statFont, Rgba32.Black, new Point(114, 120), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = level.Length
                    })

                    // Level Value
                    .DrawText(userData.Level.ToString(), statFont, Rgba32.Black, new Point(284, 120),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })

                    //Exp area
                    .Draw(Rgba32.DarkGray, 1, exp)
                    .DrawText(expstr, statFont, Rgba32.Black, new Point(114, 132), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = exp.Length
                    })

                    //Exp Value
                    .DrawText($"{userData.Xp}/{xpToLevelUp}", statFont, Rgba32.Black, new Point(284, 132),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })

                    //Total exp area
                    .Draw(Rgba32.DarkGray, 1, totalexp)
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

                    // Credit area
                    .Draw(Rgba32.DarkGray, 1, credit)
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
                        })

                    // Game info
                    // Health area
                    .Draw(Rgba32.DarkGray, 1, health)
                    .DrawText(healthstr, classFont, Rgba32.Black, new Point(114, 180), new TextGraphicsOptions(true)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Antialias = true,
                        ApplyKerning = true,
                        WrapTextWidth = health.Length
                    })
                    .DrawText($"{gameData.Health - gameData.Damagetaken}/{gameData.Health}", classFont, Rgba32.Black,
                        new Point(284, 180), new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Antialias = true,
                            ApplyKerning = true
                        })

                    // Damage area
                    .Draw(Rgba32.DarkGray, 1, damage)
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

                    // NPC kills area
                    .Draw(Rgba32.DarkGray, 1, npckill)
                    .DrawText(npckillstr, classFont, Rgba32.Black, new Point(114, 200), new TextGraphicsOptions(true)
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

                    // Fleet area
                    .Draw(Rgba32.DarkGray, 1, fleet)
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

                    // Commander area
                    .Draw(Rgba32.DarkGray, 1, commander)
                    .DrawText(commanderstr, classFont, Rgba32.Black, new Point(114, 220), new TextGraphicsOptions(true)
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

                    // Mission Completed area
                    .Draw(Rgba32.DarkGray, 1, mission)
                    .DrawText(missionstr, classFont, Rgba32.Black, new Point(114, 230), new TextGraphicsOptions(true)
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
                    })
                    // Ship Class Text
                    .DrawText($"{userData.ShipClass}", gdclassFont, Rgba32.Black, new PointF(50, 280),
                        new TextGraphicsOptions(true)
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Antialias = true,
                            ApplyKerning = true
                        })
                );
                img.Save(finalPath);
                return finalPath;
            }
        }

        private static Font GoodFont(Font font, string text, float padding, int Width, int Height)
        {
            var targetWidth = Width - padding * 2;
            var targetHeight = Height - padding * 2;

            var size = TextMeasurer.Measure(text, new RendererOptions(font));

            var scalingFactor = Math.Min(Width / size.Width, Height / size.Height);

            var scaledFont = new Font(font, scalingFactor * font.Size);

            return scaledFont;
        }

        private static string CheckFleetMembership(IUser user)
        {
            var fleetName = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            if (fleetName == null) return "N/A";
            if (fleetName.Equals("o", StringComparison.InvariantCultureIgnoreCase)) return "N/A";
            return fleetName;
        }

        private static string GetFleetCommander(IUser user, string fleet)
        {
            if (fleet.Equals("N/A", StringComparison.InvariantCultureIgnoreCase)) return "N/A";
            var commander = FleetNormDb.GetLeader(user, fleet).ToString();
            if (commander == null) return "N/A";
            return commander;
        }
    }
}