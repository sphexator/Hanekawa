using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Data;
using Hanekawa.Addons.HungerGame.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventHandler = Hanekawa.Addons.HungerGame.Events.EventHandler;

namespace Hanekawa.Addons.HungerGame
{
    public class HungerGamesClient
    {
        public async Task<HungerGameResult> HungerGameRoundAsync(DbService db, IEnumerable<HungerGameLive> profiles)
        {
            var imgGenerator = new ImageGenerator();
            var hungerGameProfiles = profiles.ToList();
            string output = null;
            foreach (var x in hungerGameProfiles)
            {
                output += $"{x.Name.PadRight(20)} {EventHandler.EventManager(x, db)}\n";
            }

            Fatigue(hungerGameProfiles);
            await db.SaveChangesAsync();
            var image = await imgGenerator.GenerateEventImageAsync(hungerGameProfiles);
            var data = new HungerGameResult
            {
                Content = output,
                Image = image,
                Participants = hungerGameProfiles
            };
            return data;
        }

        private static void Fatigue(IEnumerable<HungerGameLive> profiles)
        {
            var rand = new Random();
            foreach (var x in profiles)
            {
                x.Fatigue = x.Fatigue + rand.Next(10, 15);
                x.Hunger = x.Hunger + rand.Next(5, 10);
                x.Thirst = x.Thirst + rand.Next(10, 20);
                x.Sleep = x.Sleep + rand.Next(20, 30);
            }
        }
    }
}