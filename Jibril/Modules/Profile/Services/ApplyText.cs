﻿using Discord.WebSocket;
using Jibril.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using SixLabors.Primitives;
using System.Text;
using System.Linq;
using Jibril.Modules.Game.Services;
using Jibril.Services.Level.Lists;
using Jibril.Data.Variables;
using SixLabors.Shapes;
using SixLabors.ImageSharp.Drawing;

namespace Jibril.Modules.Profile.Services
{
    public class ApplyText
    {
        public static string ApplyTextToProfile(string filepath, SocketUser user, string randomString, UserData userData, GameStatus gameData)
        {
            var finalPath = $"Data/Images/Profile/Cache/{randomString}Final.png";
            var statFont = SystemFonts.CreateFont("Good Times Rg", 9, FontStyle.Regular);
            var classFont = SystemFonts.CreateFont("Good Times Rg", 8, FontStyle.Regular);
            using (Image<Rgba32> img = Image.Load(filepath))
            {
                // Paths
                PathBuilder usernamePath = new PathBuilder();
                PathBuilder levelPath = new PathBuilder();
                PathBuilder expPath = new PathBuilder();
                PathBuilder totalexpPath = new PathBuilder();
                PathBuilder creditPath = new PathBuilder();
                PathBuilder healthPath = new PathBuilder();
                PathBuilder damagePath = new PathBuilder();
                PathBuilder npckillPath = new PathBuilder();
                PathBuilder fleetPath = new PathBuilder();
                PathBuilder CommanderPath = new PathBuilder();
                PathBuilder missionPath = new PathBuilder();

                usernamePath.AddLine((new Point(114, 96)), (new Point(284, 96)));
                levelPath.AddLine((new Point(114, 120)), (new Point(284, 120)));
                expPath.AddLine((new Point(114, 132)), (new Point(284, 132)));
                totalexpPath.AddLine((new Point(114, 144)), (new Point(284, 144)));
                creditPath.AddLine((new Point(114, 156)), (new Point(284, 156)));
                healthPath.AddLine((new Point(114, 185)), (new Point(284, 185)));
                damagePath.AddLine((new Point(114, 195)), (new Point(284, 195)));
                npckillPath.AddLine((new Point(114, 205)), (new Point(284, 205)));
                fleetPath.AddLine((new Point(114, 215)), (new Point(284, 215)));
                CommanderPath.AddLine((new Point(114, 225)), (new Point(284, 225)));
                missionPath.AddLine((new Point(114, 235)), (new Point(284, 235)));


                IPath username = usernamePath.Build();
                IPath level = levelPath.Build();
                IPath exp = expPath.Build();
                IPath totalexp = totalexpPath.Build();
                IPath credit = creditPath.Build();
                IPath health = healthPath.Build();
                IPath damage = damagePath.Build();
                IPath npckill = npckillPath.Build();
                IPath fleet = fleetPath.Build();
                IPath commander = CommanderPath.Build();
                IPath mission = missionPath.Build();

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


                img.Mutate(x => x
                // User info
                // Username area
                //.Draw(Rgba32.Gray, 3, username)
                .DrawText($"{user.Username}", statFont, Rgba32.Black, username, new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = username.Length
                })

                // Level area
                //.Draw(Rgba32.Gray, 3, level)
                .DrawText(levelstr, statFont, Rgba32.Black, (new Point(114, 120)), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = level.Length
                })

                // Level Value
                .DrawText(userData.Level.ToString(), statFont, Rgba32.Black, new Point(284, 120), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                //Exp area
                //.Draw(Rgba32.Gray, 3, exp)
                .DrawText(expstr, statFont, Rgba32.Black, new Point(114, 132), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = exp.Length
                })

                //Exp Value
                .DrawText($"{userData.Xp}", statFont, Rgba32.Black, new Point(284, 132), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                //Total exp area
                //.Draw(Rgba32.Gray, 3, totalexp)
                .DrawText(totalexpstr, statFont, Rgba32.Black, new Point(114, 144), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = totalexp.Length
                })

                .DrawText($"{userData.Total_xp}", statFont, Rgba32.Black, new Point(284, 144), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // Credit area
                //.Draw(Rgba32.Gray, 3, credit)
                .DrawText(creditstr, statFont, Rgba32.Black, new Point(114, 156), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = credit.Length
                })

                .DrawText($"{userData.Tokens}", statFont, Rgba32.Black, new Point(284, 156), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })


                // Game info
                // Health area
                //.Draw(Rgba32.Gray, 3, health)
                .DrawText(healthstr, classFont, Rgba32.Black, new Point(114, 180), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = health.Length
                })

                .DrawText($"{gameData.Health - gameData.Damagetaken}/{gameData.Health}", classFont, Rgba32.Black, new Point(284, 180), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // Damage area
                //.Draw(Rgba32.Gray, 3, damage)
                .DrawText(damagestr, classFont, Rgba32.Black, new Point(114, 190), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = damage.Length
                })

                .DrawText($"{gameData.Damagetaken}", classFont, Rgba32.Black, new Point(284, 190), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // NPC kills area
                //.Draw(Rgba32.Gray, 3, npckill)
                .DrawText(npckillstr, classFont, Rgba32.Black, new Point(114, 200), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = npckill.Length
                })

                .DrawText($"{gameData.KillAmount}", classFont, Rgba32.Black, new Point(284, 200), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // Fleet area
                //.Draw(Rgba32.Gray, 3, fleet)
                .DrawText(fleetstr, classFont, Rgba32.Black, new Point(114, 210), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = fleet.Length
                })

                .DrawText($"{userData.FleetName}", classFont, Rgba32.Black, new Point(284, 210), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // Commander area
                //.Draw(Rgba32.Gray, 3, commander)
                .DrawText(commanderstr, classFont, Rgba32.Black, new Point(114, 220), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Antialias = true,
                    ApplyKerning = true,
                    WrapTextWidth = commander.Length
                })

                .DrawText($"{userData.FleetName}", classFont, Rgba32.Black, new Point(284, 220), new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Antialias = true,
                    ApplyKerning = true
                })

                // Mission Completed area
                //.Draw(Rgba32.Gray, 3, mission)
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

                .DrawText($"{userData.ShipClass}", classFont, Rgba32.Black, new PointF(6, 275))
                );
                img.Save(finalPath);
                return finalPath;
            }
        }
    }
}
