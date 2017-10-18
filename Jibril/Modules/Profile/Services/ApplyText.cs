using Discord.WebSocket;
using Jibril.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using SixLabors.Primitives;
using System.Text;
using System.Linq;
using Jibril.Modules.Game.Services;

namespace Jibril.Modules.Profile.Services
{
    public class ApplyText
    {
        public static string ApplyTextToProfile(string filepath, SocketUser user, string randomString)
        {
            var finalPath = $"Data/Images/Profile/Cache/{randomString}Final.png";
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            var statFont = SystemFonts.CreateFont("Good Times Rg", 9, FontStyle.Regular);
            var classFont = SystemFonts.CreateFont("Good Times Rg", 8, FontStyle.Regular);
            using (Image<Rgba32> img = Image.Load(filepath))
            {
                img.Mutate(x => x
                // User info
                // Username area
                .DrawText($"{user.Username}", statFont, Rgba32.Black, new PointF(115, 115))
                .DrawText($"{userData.Level}", statFont, Rgba32.Black, new PointF(165, 115))

                // Level area
                .DrawText("LEVEL", statFont, Rgba32.Black, new PointF(115, 115))
                .DrawText($"{userData.Level}", statFont, Rgba32.Black, new PointF(165, 115))

                //Exp area
                .DrawText("EXP", statFont, Rgba32.Black, new PointF(115, 127))
                .DrawText($"{userData.Level}", statFont, Rgba32.Black, new PointF(165, 127))

                //Total exp area
                .DrawText("TOTAL EXP", statFont, Rgba32.Black, new PointF(115, 139))
                .DrawText($"{userData.Level}", statFont, Rgba32.Black, new PointF(165, 139))

                // Credit area
                .DrawText("Credit", statFont, Rgba32.Black, new PointF(115, 151))
                .DrawText($"{userData.Level}", statFont, Rgba32.Black, new PointF(165, 151))


                // Game info
                // Health area
                .DrawText("Health", classFont, Rgba32.Black, new PointF(115, 180))
                .DrawText($"{gameData.Health - gameData.Damagetaken}/{gameData.Health}", classFont, Rgba32.Black, new PointF(165, 180))

                // Damage area
                .DrawText("Damage", classFont, Rgba32.Black, new PointF(115, 190))
                .DrawText($"{gameData.Damagetaken}", classFont, Rgba32.Black, new PointF(165, 190))

                // NPC kills area
                .DrawText("NPC Kills", classFont, Rgba32.Black, new PointF(115, 200))
                .DrawText($"{gameData.KillAmount}", classFont, Rgba32.Black, new PointF(165, 200))

                // Fleet area
                .DrawText("Fleet", classFont, Rgba32.Black, new PointF(115, 210))
                .DrawText($"{userData.FleetName}", classFont, Rgba32.Black, new PointF(165, 210))

                // Commander area
                .DrawText("Commander", classFont, Rgba32.Black, new PointF(115, 220))
                .DrawText($"{userData.FleetName}", classFont, Rgba32.Black, new PointF(165, 220))

                // Mission Completed area
                .DrawText("Missions Completed", classFont, Rgba32.Black, new PointF(115, 230))
                .DrawText($"0", classFont, Rgba32.Black, new PointF(165, 230))

                .DrawText($"{userData.ShipClass}", classFont, Rgba32.Black, new PointF(6, 275))
                );
                img.Save(finalPath);
                return finalPath;
            }
        }
    }
}
