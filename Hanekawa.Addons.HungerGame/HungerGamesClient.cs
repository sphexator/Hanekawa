using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Data;
using Hanekawa.Addons.HungerGame.Generator;
using EventHandler = Hanekawa.Addons.HungerGame.Events.EventHandler;

namespace Hanekawa.Addons.HungerGame
{
    public class HungerGamesClient
    {
        public HungerGamesClient(DiscordSocketClient client)
        {
            _client = client;
        }

        private DiscordSocketClient _client { get; }

        public async Task<HungerGameResult> HungerGameRoundAsync(DbService db, IEnumerable<HungerGameLive> profiles)
        {
            var imgGenerator = new ImageGenerator(_client);
            var hungerGameProfiles = profiles.ToList();
            string output = null;
            foreach (var x in hungerGameProfiles)
            {
                    if (!x.Status) continue;
                    var eventString = EventHandler.EventManager(x, db);
                    if (eventString == null) continue;
                try
                {
                    var name = x.Name;
                    output += $"{name.PadRight(20)} {eventString}\n";
                }
                catch
                {
                    output += $"{x.Name.PadRight(20)} {eventString}\n";
                }
            }

            SelfHarm(hungerGameProfiles);
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
                if (!x.Status) continue;
                x.Hunger = x.Hunger + rand.Next(5, 10);
                x.Sleep = x.Sleep + rand.Next(20, 30);
            }
        }

        private static void SelfHarm(IEnumerable<HungerGameLive> profiles)
        {
            var rand = new Random();
            foreach (var x in profiles)
            {
                if (!x.Status) continue;
                int dmg;
                if (x.Hunger >= 90 || x.Sleep >= 100) dmg = rand.Next(20, 30);
                else if (x.Hunger >= 80 || x.Sleep >= 90) dmg = rand.Next(5, 10);
                else continue;
                if (x.Health - dmg <= 0)
                {
                    x.Status = false;
                    x.Health = 0;
                }
                else
                {
                    x.Health = x.Health - dmg;
                }
            }
        }
    }
}